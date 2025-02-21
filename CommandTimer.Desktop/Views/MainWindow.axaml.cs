using Avalonia.Controls;
using Avalonia.Interactivity;
using CommandTimer.Core;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.Utilities.DependencyInversion;
using CommandTimer.Core.ViewModels;
using CommandTimer.Desktop.Utilities;

namespace CommandTimer.Desktop.Views;

public partial class MainWindow : Window {

    public static Window? Instance { get; private set; }
    public static Panel? Layout { get; private set; }

    public MainWindow() {
        Instance = this;
        Layout = ContentPresenter;

        ServiceProvider.Set<IShowToolTip>(RegisterCustomToolTip());
        ServiceProvider.Set<IAskTheUser>(new AskTheUserForMe());
        ServiceProvider.Set<IDefaultTimerCollection>(new DefaultTimers_Simple(SystemInteraction.Platform.Get()));
        var passwordManager = new PasswordManager(ServiceProvider.Get<ISerializer>(), [
                new PasswordRule_MinimumCharacters(4),
            ]);
        ServiceProvider.Set<IPasswordValidation>(passwordManager);
        ServiceProvider.Set<IPasswordFormatValidation>(passwordManager);

        InitializeComponent();

        bool? terms = StartupDisclaimer.HaveShownTerms();
        if (terms is null || terms is false) {
            _ = StartupDisclaimer.Create().Show(MainWindowLayout);

            AddDefaultTimers();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);

        ClientAreaExtendedToSystemChromeSpace.Height = this.WindowDecorationMargin.Top;
    }

    private CustomTipTimer RegisterCustomToolTip() {
        var view = new CustomToolTip();
        return new CustomTipTimer(this, view, configureOnShow, 1400);

        void configureOnShow(string tooltip, IShowToolTipProperties properties) {
            view.PART_TextPresenter.Text = tooltip;
            view.PART_BorderBackground.Background = properties.Background;
            view.Placement = properties.Reference.Placement;
            view.PlacementAnchor = properties.Reference.PlacementAnchor;
            view.VerticalOffset = properties.Reference.VerticalOffset;
            view.HorizontalOffset = properties.Reference.HorizontalOffset;
        }
    }

    private static void AddDefaultTimers() {
        LibraryManager.LoadLibraryToCurrent(LibraryManager.DEFAULT_LIBRARY);
        ServiceProvider.Get<IDefaultTimerCollection>().Timers
                       .ForEach(timer => LibraryManager.GetLibrary(timer.LibraryName).AddToLibrary(timer));
    }
}
