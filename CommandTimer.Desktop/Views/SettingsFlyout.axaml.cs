using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.Utilities.ExtensionMethods;
using CommandTimer.Core.ViewModels.MenuItems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandTimer.Desktop.Views;

public partial class SettingsFlyout : UserControl {

    private static ILibraryManager LibraryManager => ServiceProvider.Get<ILibraryManager>();

    private FlyoutBase? _colorBarFlyout;
    private readonly List<(Control Control, EventHandler<PointerEventArgs> Handler)> _toolTipPointerEnteredHandlers = [];
    private EventHandler<PointerPressedEventArgs>? _toolTipPointerPressedHandler;

    public SettingsFlyout() {
        InitializeComponent();

        /// Settings Flyout
        var settingsData = ServiceProvider.Get<ISerializer>().Deserialize<SettingsFlyoutData>(Settings.Keys.GlobalSettings, Settings.DEFAULT_DATA_FILE)
            ?? new SettingsFlyoutData();
        DataContext = new SettingsFlyoutViewModel(settingsData);
    }

    protected override void OnLoaded(RoutedEventArgs e) {

        if (Design.IsDesignMode is false) {
            SetupCustomToolTips();
        }

        _colorBarFlyout = FlyoutBase.GetAttachedFlyout(ColorBar);
        if (_colorBarFlyout is not null) {
            _colorBarFlyout.Opened += (s, e) => {
                if (TopLevel.GetTopLevel(this) is MainWindow window) {
                    window.AddHandler(Window.KeyDownEvent, ColorBar_KeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
                }
            };
            _colorBarFlyout.Closed += (s, e) => {
                if (TopLevel.GetTopLevel(this) is MainWindow window) {
                    window.RemoveHandler(Window.KeyDownEvent, ColorBar_KeyDown);
                }
            };
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        ClearCustomToolTipHandlers();
        ColorPickerControl.KeyDown -= ColorBar_KeyDown;
    }

    private async void Tapped_StartAutoStartsButton(object? sender, TappedEventArgs args) {
        if (TopLevel.GetTopLevel(this) is not MainWindow window) return;

        try {
            List<CommandTimerViewModel> autoTimers = [];
            foreach (var library in LibraryManager.Libraries) {
                autoTimers.AddRange(library.CommandTimers.Where(t => t.IsAutoStart));
            }
            bool? result = await ConfirmationDialog.Create()
                                                   .WithMessage($"This will start {autoTimers.Count} timer{(autoTimers.Count == 1 ? "" : "s")}. {Environment.NewLine}{Environment.NewLine} Are you sure you want to do this?")
                                                   .Show(window.MainWindowLayout);
            if (result is false) return;

            LibraryManager.TriggerAutoStarts(true);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(Tapped_StartAutoStartsButton)} has thrown the following.", ex);
        }
    }

    private async void Tapped_PromptPasswordButton(object? sender, TappedEventArgs args) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;
        if (TopLevel.GetTopLevel(this) is not MainWindow window) return;
        if (viewModel.ShouldUsePasswordConfirmation) {
            viewModel.ShouldUsePasswordConfirmation = false;
            return;
        }
        var passwordValidation = ServiceProvider.Get<IPasswordValidation>();
        var passwordFormat = ServiceProvider.Get<IPasswordFormatValidation>();

        try {
            if (passwordValidation.IsSet()) {
                /// Ask for previous password
                var previousPasswordDialog = ConfirmationDialog.Create()
                                                               .WithMessage($"You will be asked to confirm.{Environment.NewLine}Enter a new password:")
                                                               .WithPasswordEntry();

                bool? previous = await previousPasswordDialog.Show(window.MainWindowLayout);
                if (previous.Value is false) return;

                var decisionOnPrevious = passwordValidation.Validate(previousPasswordDialog.PART_PasswordEntry.Text ?? string.Empty);
                if (decisionOnPrevious is false) return;
            }

            /// Ask for a new password
            var askPasswordDialog = ConfirmationDialog.Create()
                                                      .WithMessage($"You will be asked to confirm.{Environment.NewLine}Enter a new password:")
                                                      .WithPasswordEntry(true, s => true)
                                                      .WithPasswordValidationFeedback();

            bool? ask = await askPasswordDialog.Show(window.MainWindowLayout);
            if (ask is false) return;

            var firstEntry = askPasswordDialog.PART_PasswordEntry.Text ?? string.Empty;

            /// Confirm the password
            bool? confirm = await ConfirmationDialog.Create()
                                                    .WithMessage($"Enter your new password again:")
                                                    .WithPasswordEntry(true, (s) => string.Equals(s, firstEntry))
                                                    .WithBorder(Brushes.Gold, new Thickness(1))
                                                    .Show(window.MainWindowLayout);
            if (confirm is false) return;
            passwordValidation.Store(firstEntry);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(Tapped_PromptPasswordButton)} has thrown the following.", ex);
        }

        viewModel.ShouldUsePasswordConfirmation = true;
    }

    private async void Tapped_StopAutoStartsButton(object? sender, TappedEventArgs args) {
        if (TopLevel.GetTopLevel(this) is not MainWindow window) return;

        try {
            List<CommandTimerViewModel> autoTimers = [];
            foreach (var library in LibraryManager.Libraries) {
                autoTimers.AddRange(library.CommandTimers.Where(t => t.IsAutoStart && t.IsActive));
            }
            bool? result = await ConfirmationDialog.Create()
                                                   .WithMessage($"This will stop {autoTimers.Count} running timer{(autoTimers.Count == 1 ? "" : "s")}. \r\n Are you sure you want to do this?")
                                                   .Show(window.MainWindowLayout);
            if (result is false) return;

            LibraryManager.TriggerAutoStarts(false);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(Tapped_StopAutoStartsButton)} has thrown the following.", ex);
        }
    }

    private async void Tapped_RestoreBackupButton(object? sender, TappedEventArgs args) {
        if (TopLevel.GetTopLevel(this) is not MainWindow window) return;

        try {
            bool? result = await ConfirmationDialog.Create()
                                                   .WithMessage($"Restore previous data? Current data will be lost.{Environment.NewLine}{Environment.NewLine}Are you sure you want to do this?")
                                                   .Show(window.MainWindowLayout);
            if (result is false) return;

            var serializer = ServiceProvider.Get<ISerializer>();
            serializer.RestoreFile(Settings.DEFAULT_DATA_FILE);
            LibraryManager.Libraries.ForEach(l => serializer.RestoreFile(l.LibraryName));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(Tapped_RestoreBackupButton)} has thrown the following.", ex);
        }
    }

    private async void Tapped_CreateBackupButton(object? sender, TappedEventArgs args) {
        if (TopLevel.GetTopLevel(this) is not MainWindow window) return;

        try {
            bool? result = await ConfirmationDialog.Create()
                                                   .WithMessage($"This will create a new backup, causing the oldest to be deleted.{Environment.NewLine}{Environment.NewLine}Are you sure you want to do this?")
                                                   .Show(window.MainWindowLayout);
            if (result is false) return;

            LibraryManager.BackupLibraries();
            LibraryManager.CleanLibraries();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(Tapped_CreateBackupButton)} has thrown the following.", ex);
        }
    }

    private async void Tapped_EraseBackupsButton(object? sender, TappedEventArgs args) {
        if (TopLevel.GetTopLevel(this) is not MainWindow window) return;

        try {
            List<CommandTimerViewModel> autoTimers = [];
            foreach (var library in LibraryManager.Libraries) {
                autoTimers.AddRange(library.CommandTimers.Where(t => t.IsAutoStart && t.IsActive));
            }
            bool? result = await ConfirmationDialog.Create()
                                                   .WithMessage($"This will delete all backup files. Settings along with old and new libraries are preserved. \r\n Are you sure you want to do this?")
                                                   .Show(window.MainWindowLayout);
            if (result is false) return;

            ServiceProvider.Get<ISerializer>().DeleteBackupFiles();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(Tapped_EraseBackupsButton)} has thrown the following.", ex);
        }
    }

    private void Tapped_ThemeSelection(object? sender, TappedEventArgs args) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        UpdateControl_MenuItem(viewModel.ThemeSelections, viewModel.ThemeSelection);
    }

    /// <summary>
    /// Keep menu items in sync as a whole.
    /// </summary>
    private static void UpdateControl_MenuItem(IEnumerable<MenuItemViewModel> menuItems, MenuItemViewModel selected) {
        foreach (var item in menuItems) {
            item.IsSelected = item == selected;
            item.BackgroundColor = item.IsSelected ? ServiceProvider.Get<IColorProvider>().ApplicationBrush_Accent.Value.AsBrush() : ServiceProvider.Get<IColorProvider>().ApplicationBrush_Transparent.Value.AsBrush();
        }
    }


    //... Accent Color Picker

    public void Changed_ColorPick(object sender, ColorChangedEventArgs args) => Sync_ColorPick();
    private void Sync_ColorPick() {
        ColorApplyButton.Background = new SolidColorBrush(ColorPickerControl.Color);
        ColorApplyButton.Foreground = new SolidColorBrush(ColorUtilities.GetSlidingContrastColor(new AppColor(ColorPickerControl.Color.A, ColorPickerControl.Color.R, ColorPickerControl.Color.G, ColorPickerControl.Color.B)).AsBrush().Color);
    }

    private void ColorPick_KeyDown(object sender, KeyEventArgs args) {
        if (args.Key == Key.Enter) {
            ColorPick_ApplyColor();
            args.Handled = true;
        }
        if (args.Key == Key.Escape) {
            ColorPick_CancelColor();
            args.Handled = true;
        }
    }

    private void Tapped_ColorBar(object sender, TappedEventArgs args) {
        Sync_ColorPick();
        var colorBarFlyout = FlyoutBase.GetAttachedFlyout(ColorBar);
        colorBarFlyout?.ShowAt(ColorBar);
    }

    private void ColorBar_KeyDown(object? sender, KeyEventArgs args) {
        if (args.Key == Key.Enter) {
            ColorPick_ApplyColor();
            args.Handled = true;
        }
        if (args.Key == Key.Escape) {
            ColorPick_CancelColor();
            args.Handled = true;
        }
    }

    private void Tapped_ApplyColor(object sender, TappedEventArgs args) => ColorPick_ApplyColor();
    private void ColorPick_ApplyColor() {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        viewModel.AccentColorSelection = new AppColor(ColorPickerControl.Color.A, ColorPickerControl.Color.R, ColorPickerControl.Color.G, ColorPickerControl.Color.B);
        var colorBarFlyout = FlyoutBase.GetAttachedFlyout(ColorBar);
        colorBarFlyout?.Hide();
    }

    private void Tapped_CancelColor(object sender, TappedEventArgs args) => ColorPick_CancelColor();
    private void ColorPick_CancelColor() {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        ColorPickerControl.Color = viewModel.AccentColorSelection.AsBrush().Color;
        FlyoutBase.GetAttachedFlyout(ColorBar)?.Hide();
    }

    private void Tapped_ResetAccentColor(object sender, TappedEventArgs args) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        var accentKey = nameof(IColorProvider.ApplicationBrush_Accent);
        if (ServiceProvider.Get<IColorProvider>() is CommandTimer.Desktop.Utilities.ColorProvider.AvaloniaColorProvider provider
            && provider.TryGetDefaultBrush(accentKey, out var defaultBrush)) {
            viewModel.AccentColorSelection = new AppColor(defaultBrush.Color.A, defaultBrush.Color.R, defaultBrush.Color.G, defaultBrush.Color.B);
        }
    }

    private void Tapped_UseSystemColor(object sender, TappedEventArgs args) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        if (Application.Current?.Styles.FirstOrDefault(s => s is FluentTheme) is FluentTheme theme) {
            if (Application.Current.TryGetResource("SystemAccentColor", out object? accentColorObject) && accentColorObject is Avalonia.Media.Color systemAccentColor) {
                viewModel.AccentColorSelection = new AppColor(systemAccentColor.A, systemAccentColor.R, systemAccentColor.G, systemAccentColor.B);
            }
        }
    }

    private void PointerEntered_ColorBar(object? sender, PointerEventArgs args) => ColorBar.BorderThickness = new Thickness(1);
    private void PointerExited_ColorBar(object? sender, PointerEventArgs args) => ColorBar.BorderThickness = new Thickness(0);

    private void SetupCustomToolTips() {
        ClearCustomToolTipHandlers();

        var properties = ToolTipDefaults.GetDefaults();
        properties.Reference.Placement = PlacementMode.Bottom;
        var tooltip = ServiceProvider.Get<IShowToolTip>();

        _toolTipPointerPressedHandler = (o, a) => tooltip.Hide();
        RootPanel.AddHandler(PointerPressedEvent, _toolTipPointerPressedHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);

        RegisterToolTip(ThemeButtonTop, "Light Mode | Dark Mode");
        RegisterToolTip(ShouldAnimateCheckBox, "Show animations on list actions");
        RegisterToolTip(ShouldExecuteOnTimerCheckBox, "Manual executions only");
        RegisterToolTip(ShouldNotifExpireCheckBox, "A timer execution notification will expire by default. Switch this off to preserve notifications for manual dismissal. (So you can see what happened while away.)");
        RegisterToolTip(ShouldAutoStartCheckBox, "Globally control auto start at program startd");
        RegisterToolTip(StartAutoStartButton, "Start all auto start timers");
        RegisterToolTip(StopAutoStartButton, "Stop all auto start timers");
        RegisterToolTip(ShouldExpandColorBarCheckBox, "Expand Color Bar color to shade command field");
        RegisterToolTip(ShouldStripeCheckBox, "Show alternating background colors for list items");
        RegisterToolTip(ShouldLogCheckBox, "Log commands executed and their outputs");
        RegisterToolTip(ShouldPromptCheckBox, "Prompt before manual execution");
        RegisterToolTip(ShouldCleanDatabaseCheckBox, "Current data is moved to a backup and a fresh copy is sorted before beginning");
        RegisterToolTip(RestoreBackupButton, "Restore the previous backup");
        RegisterToolTip(CreateBackupButton, "Perform backup and data cleaning");
        RegisterToolTip(EraseBackupsButton, "Erase all backups");
        RegisterToolTip(BackupVersionsSlider, "Choose how many save data versions to keep after database cleaning");
        RegisterToolTip(MaxLinesSlider, "Choose how many lines to expand for large descriptions");
        RegisterToolTip(ColorBar, "Select an accent color");
        RegisterToolTip(ResetColorButton, "Reset the accent color to the application default accent color");
        RegisterToolTip(SystemColorButton, "Reset the accent color to your system accent color");
        RegisterToolTip(PromptPasswordButton, "Require a password for confirmation windows");

        void RegisterToolTip(Control control, string message) {
            EventHandler<PointerEventArgs> handler = (s, args) => tooltip.OnPointerOver(control, message, properties);
            control.PointerEntered += handler;
            _toolTipPointerEnteredHandlers.Add((control, handler));
        }
    }

    private void ClearCustomToolTipHandlers() {
        if (_toolTipPointerPressedHandler is not null) {
            RootPanel.RemoveHandler(PointerPressedEvent, _toolTipPointerPressedHandler);
            _toolTipPointerPressedHandler = null;
        }

        foreach (var (control, handler) in _toolTipPointerEnteredHandlers) {
            control.PointerEntered -= handler;
        }

        _toolTipPointerEnteredHandlers.Clear();
    }
}


