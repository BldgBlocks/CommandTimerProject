using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using CommandTimer.Core;
using CommandTimer.Core.Converters;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.ViewModels;
using CommandTimer.Desktop.Views;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CommandTimer.Desktop;

public partial class App : Application {

    private static bool _willCommit;

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);

        // Enable DevTools if in Debug mode.
#if DEBUG
        this.AttachDevTools();
#endif
    }

    /// <summary>
    /// Shutdown the application.
    /// </summary>
    public static void Shutdown(int code = 0) {
        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.Shutdown(code);
        }
    }

    public static async void CopyToClipboard(string text) {
        if (TopLevel.GetTopLevel(MainWindow.Instance) is not TopLevel topLevel) return;
        if (topLevel.Clipboard is null) return;
        try {
            await topLevel.Clipboard.SetTextAsync(text);
        }
        /// Disregard clipboard exceptions
        catch (Exception) { }
    }

    public static Task<string?> CopyFromClipboardAsync<T>() {
        if (TopLevel.GetTopLevel(MainWindow.Instance) is not TopLevel topLevel) return Task.FromResult<string?>(string.Empty);
        if (topLevel.Clipboard is null) return Task.FromResult<string?>(string.Empty);

        return topLevel.Clipboard.GetTextAsync();
    }

    public override void OnFrameworkInitializationCompleted() {
        /// Line below is needed to remove Avalonia data validation.
        /// Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        /// Services
        SetupSerialization();

        MessageLogger.Create(SystemInteraction.Files.GetPlatformLogFile()).StartLogging();

        /// Exceptions
        SetupExceptionHandling();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            var mainWindow = new MainWindow {
                DataContext = new MainWindowModel()
            };
            desktop.MainWindow = mainWindow;

            desktop.Startup += (sender, e) => {
                // Here you can also catch startup-specific issues or set up additional handlers if needed.
            };
            desktop.Exit += (sender, e) => {
                ServiceProvider.Get<ISerializer>().Commit();
            };
        }

        if (Design.IsDesignMode is false) {
            LibraryManager.CleanDatabase();
        }

        /// Fluent Theme Accent Color
        var observable = Core.Settings.AccentColorSelection;
        FluentTheme_UpdateAccentColor(ThemeVariant.Dark, observable.Value.Color);
        FluentTheme_UpdateAccentColor(ThemeVariant.Light, observable.Value.Color);
        observable.ValueChanged += (brush) => {
            FluentTheme_UpdateAccentColor(ThemeVariant.Dark, brush.Color);
            FluentTheme_UpdateAccentColor(ThemeVariant.Light, brush.Color);
        };

        /// Loading

        base.OnFrameworkInitializationCompleted();
    }

    private static void FluentTheme_UpdateAccentColor(ThemeVariant theme, Color color) {
        if (Application.Current?.Styles.Where(s => s is FluentTheme).FirstOrDefault() is not FluentTheme fluentTheme) return;
        if (fluentTheme.Palettes.TryGetValue(theme, out var colorPaletteResources) is false) return;

        colorPaletteResources.Accent = color;
    }

    public static Styles LoadStyleResource(string avaloniaAssemblyPath = "avares://", string styleResourcePath = "myStyle.axaml") {
        var styles = new Styles();
        var styleInclude = new StyleInclude(new Uri(avaloniaAssemblyPath, UriKind.Absolute)) {
            Source = new Uri(styleResourcePath)
        };
        styles.Add(styleInclude);
        return styles;
    }

    private void SetupExceptionHandling() {
        // Handle UI thread exceptions
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
            if (e.ExceptionObject is Exception ex) {
                HandleException("AppDomain Unhandled Exception", ex);
            }
            else {
                // If for some reason it's not an Exception, still log it or handle it as best you can.
                HandleException("AppDomain Unhandled Exception", new Exception(e.ExceptionObject.ToString()));
            }

            // Note: Once this event is fired, the application 
            // will terminate after this event handler completes.
        };

        // Handle TaskScheduler exceptions if using TPL
        TaskScheduler.UnobservedTaskException += (sender, e) => {
            HandleException("Unobserved Task Exception", e.Exception);
            e.SetObserved();
        };
    }

    private void HandleException(string context, Exception exception) {
        var message = FlattenException(exception);

        /// Avalonia Dbus nuisance
        if (exception is AggregateException 
            && message.Contains("com.canonical.AppMenu.Registrar")
            || message.Contains("The name is not activatable")) {
            /// Avalonia Dbus integration is causing exceptions.
            /// Ignore the exception.
            return;
        }

        /// Here you log, display error messages, or attempt recovery
        LogException(context, exception);
        ShowUserError($"Source: {exception.Source} {message}");
        /// Optionally, decide if you can continue or should initiate shutdown
    }

    private void LogException(string context, Exception ex) {
        /// Implement your logging here

    }

    private static string FlattenException(Exception exception) {
        if (exception == null) return string.Empty;

        var sb = new StringBuilder();
        AppendExceptionDetails(sb, exception);
        return sb.ToString();
    }

    private static void AppendExceptionDetails(StringBuilder sb, Exception exception) {
        sb.AppendLine($"Type: {exception.GetType().FullName}");
        sb.AppendLine($"Message: {exception.Message}");
        sb.AppendLine($"StackTrace: {exception.StackTrace}");

        if (exception.InnerException != null) {
            sb.AppendLine("Inner Exception:");
            AppendExceptionDetails(sb, exception.InnerException);
        }
    }

    private void ShowUserError(string message) {
        int limit = 3000;
        Core.MessageRelay.OnMessagePosted(this, message.Length > limit ? message[..limit] : message);
    }

    private static void SetupSerialization() {
        JsonSerializerOptions jsonOptions = new() {
            WriteIndented = true,
            IncludeFields = true,
            Converters = {
                new JsonConverter_SolidColorBrush(),
            }
        };

        CacheSerializer serializer = new(jsonOptions,
                                         Core.Settings.BackupVersionsToKeep.GetValue,
                                         SystemInteraction.Files.GetUserConfigPath,
                                         (message) => Core.MessageRelay.OnMessagePosted(nameof(CacheSerializer), message, Core.MessageRelay.MessageCategory.Exception),
                                         onCommit);

        ServiceProvider.Set<ISerializer>(serializer);

        static async void onCommit(ISerializer serializer) {
            if (_willCommit) return;
            _willCommit = true;

            try {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    /// Next cycle, write.
                    await Task.Yield();

                    serializer.Commit();
                    _willCommit = false;
                }, DispatcherPriority.Background);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) {
                throw new Exception($"Method: {nameof(onCommit)} has thrown the following.", ex);
            }
            finally {
                _willCommit = false;
            }
        }
    }
}
