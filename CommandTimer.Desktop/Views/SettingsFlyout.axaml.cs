using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using CommandTimer.Core;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.Utilities.DependencyInversion;
using CommandTimer.Core.ViewModels;
using CommandTimer.Core.ViewModels.MenuItems;
using CommandTimer.Desktop.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static CommandTimer.Core.Settings;

namespace CommandTimer.Desktop.Views;

public partial class SettingsFlyout : UserControl {

    private FlyoutBase? _colorBarFlyout;

    public SettingsFlyout() {
        InitializeComponent();

        /// Settings Flyout
        if (ServiceProvider.Get<ISerializer>().Deserialize<SettingsFlyoutViewModel>(Core.Settings.Keys.GlobalSettings, Core.Settings.DEFAULT_DATA_FILE) is not SettingsFlyoutViewModel settingsViewModel) {
            settingsViewModel = new SettingsFlyoutViewModel();
        }
        DataContext = settingsViewModel;
    }

    protected override void OnLoaded(RoutedEventArgs e) {

        if (Design.IsDesignMode is false) {
            SetupCustomToolTips();
        }

        /// Bind changes from the static settings class even though the view model will likely be authoritative and serialized.
        Core.Settings.ShouldAnimate.ValueChanged += ShouldAnimate_ValueChanged;
        Core.Settings.ShouldExecuteOnTimer.ValueChanged += ShouldExecuteOnTimer_ValueChanged;
        Core.Settings.ShouldLog.ValueChanged += ShouldLog_ValueChanged;
        Core.Settings.ShouldPromptByDefault.ValueChanged += ShouldPromptByDefault_ValueChanged;
        Core.Settings.ShouldAutoNotificationsExpire.ValueChanged += ShouldAutoNotificationsExpire_ValueChanged;
        Core.Settings.ShouldExpandColorBar.ValueChanged += ShouldExpandColorBar_ValueChanged;
        if (DataContext is SettingsFlyoutViewModel viewModel) {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        _colorBarFlyout = FlyoutBase.GetAttachedFlyout(ColorBar);
        if (_colorBarFlyout is not null) {
            _colorBarFlyout.Opened += (s, e) => {
                if (MainWindow.Instance is MainWindow window) {
                    window.AddHandler(Window.KeyDownEvent, ColorBar_KeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
                }
            };
            _colorBarFlyout.Closed += (s, e) => {
                if (MainWindow.Instance is MainWindow window) {
                    window.RemoveHandler(Window.KeyDownEvent, ColorBar_KeyDown);
                }
            };
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        Core.Settings.ShouldAnimate.ValueChanged -= ShouldAnimate_ValueChanged;
        Core.Settings.ShouldExecuteOnTimer.ValueChanged -= ShouldExecuteOnTimer_ValueChanged;
        Core.Settings.ShouldLog.ValueChanged -= ShouldLog_ValueChanged;
        Core.Settings.ShouldPromptByDefault.ValueChanged -= ShouldPromptByDefault_ValueChanged;
        Core.Settings.ShouldAutoNotificationsExpire.ValueChanged -= ShouldAutoNotificationsExpire_ValueChanged;
        Core.Settings.ShouldExpandColorBar.ValueChanged -= ShouldExpandColorBar_ValueChanged;
        if (DataContext is SettingsFlyoutViewModel viewModel) {
            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        ColorPickerControl.KeyDown -= ColorBar_KeyDown;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs args) {

    }

    private void ShouldPromptByDefault_ValueChanged(bool value) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        viewModel.ShouldPromptByDefault = value;
    }

    private void ShouldLog_ValueChanged(bool value) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        viewModel.ShouldLog = value;
    }

    private void ShouldExecuteOnTimer_ValueChanged(bool value) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        viewModel.ShouldExecuteOnTimer = value;
    }

    private void ShouldAutoNotificationsExpire_ValueChanged(bool value) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        viewModel.ShouldAutoNotificationsExpire = value;
    }

    private void ShouldAnimate_ValueChanged(AnimationChoice value) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        viewModel.ShouldAnimate = value is AnimationChoice.All;
    }

    private async void Tapped_StartAutoStartsButton(object? sender, TappedEventArgs args) {
        if (MainWindow.Instance is not MainWindow window) return;

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

    private void ShouldExpandColorBar_ValueChanged(bool value) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        viewModel.ShouldExpandColorBar = value;
    }

    private async void Tapped_PromptPasswordButton(object? sender, TappedEventArgs args) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;
        if (MainWindow.Instance is not MainWindow window) return;
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
        if (MainWindow.Instance is not MainWindow window) return;

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
        if (MainWindow.Instance is not MainWindow window) return;

        try {
            bool? result = await ConfirmationDialog.Create()
                                                   .WithMessage($"Restore previous data? Current data will be lost.{Environment.NewLine}{Environment.NewLine}Are you sure you want to do this?")
                                                   .Show(window.MainWindowLayout);
            if (result is false) return;

            var serializer = ServiceProvider.Get<ISerializer>();
            serializer.RestoreFile(DEFAULT_DATA_FILE);
            LibraryManager.Libraries.ForEach(l => serializer.RestoreFile(l.LibraryName));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(Tapped_RestoreBackupButton)} has thrown the following.", ex);
        }
    }

    private async void Tapped_CreateBackupButton(object? sender, TappedEventArgs args) {
        if (MainWindow.Instance is not MainWindow window) return;

        try {
            bool? result = await ConfirmationDialog.Create()
                                                   .WithMessage($"This will create a new backup, causing the oldest to be deleted.{Environment.NewLine}{Environment.NewLine}Are you sure you want to do this?")
                                                   .Show(window.MainWindowLayout);
            if (result is false) return;

            LibraryManager.CleanDatabase();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(Tapped_CreateBackupButton)} has thrown the following.", ex);
        }
    }

    private async void Tapped_EraseBackupsButton(object? sender, TappedEventArgs args) {
        if (MainWindow.Instance is not MainWindow window) return;

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
            item.BackgroundColor = item.IsSelected ? Core.Colors.ApplicationBrush_Accent : Core.Colors.ApplicationBrush_Transparent;
        }
    }


    //... Accent Color Picker

    public void Changed_ColorPick(object sender, ColorChangedEventArgs args) => Sync_ColorPick();
    private void Sync_ColorPick() {
        ColorApplyButton.Background = new SolidColorBrush(ColorPickerControl.Color);
        ColorApplyButton.Foreground = new SolidColorBrush(Core.Colors.GetSlidingContrastColor(ColorPickerControl.Color));
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

        viewModel.AccentColorSelection = new SolidColorBrush(ColorPickerControl.Color);
        var colorBarFlyout = FlyoutBase.GetAttachedFlyout(ColorBar);
        colorBarFlyout?.Hide();
    }

    private void Tapped_CancelColor(object sender, TappedEventArgs args) => ColorPick_CancelColor();
    private void ColorPick_CancelColor() {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        ColorPickerControl.Color = viewModel.AccentColorSelection.Color;
        FlyoutBase.GetAttachedFlyout(ColorBar)?.Hide();
    }

    private void Tapped_ResetAccentColor(object sender, TappedEventArgs args) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        var accentBrush = nameof(Core.Colors.ApplicationBrush_Accent);
        if (Core.Colors.TryGetDefaultBrush(accentBrush, out var defaultBrush)) {
            viewModel.AccentColorSelection = defaultBrush;
        }
    }

    private void Tapped_UseSystemColor(object sender, TappedEventArgs args) {
        if (DataContext is not SettingsFlyoutViewModel viewModel) return;

        if (Application.Current?.Styles.FirstOrDefault(s => s is FluentTheme) is FluentTheme theme) {
            if (Application.Current.TryGetResource("SystemAccentColor", out object? accentColorObject) && accentColorObject is Color systemAccentColor) {
                viewModel.AccentColorSelection = new SolidColorBrush(systemAccentColor);
            }
        }
    }

    private void PointerEntered_ColorBar(object? sender, PointerEventArgs args) => ColorBar.BorderThickness = new Thickness(1);
    private void PointerExited_ColorBar(object? sender, PointerEventArgs args) => ColorBar.BorderThickness = new Thickness(0);

    private void SetupCustomToolTips() {
        var properties = ToolTipDefaults.GetDefaults();
        properties.Reference.Placement = PlacementMode.Bottom;
        var tooltip = ServiceProvider.Get<IShowToolTip>();

        RootPanel.AddHandler(PointerPressedEvent, (o, a) => tooltip.Hide(), RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);

        ThemeButtonTop.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Light Mode | Dark Mode", properties);

        ShouldAnimateCheckBox.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Show animations on list actions", properties);

        ShouldExecuteOnTimerCheckBox.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Manual executions only", properties);

        ShouldNotifExpireCheckBox.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!,
            "A timer execution notification will expire by default. Switch this off to preserve notifications for manual dismissal. (So you can see what happened while away.)", properties);

        ShouldAutoStartCheckBox.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Globally control auto start at program startd", properties);

        StartAutoStartButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Start all auto start timers", properties);

        StopAutoStartButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Stop all auto start timers", properties);

        ShouldExpandColorBarCheckBox.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Expand Color Bar color to shade command field", properties);

        ShouldStripeCheckBox.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Show alternating background colors for list items", properties);

        ShouldLogCheckBox.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Log commands executed and their outputs", properties);

        ShouldPromptCheckBox.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Prompt before manual execution", properties);

        ShouldCleanDatabaseCheckBox.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Current data is moved to a backup and a fresh copy is sorted before beginning", properties);

        RestoreBackupButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Restore the previous backup", properties);

        CreateBackupButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Perform backup and data cleaning", properties);

        EraseBackupsButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Erase all backups", properties);

        BackupVersionsSlider.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Choose how many save data versions to keep after database cleaning", properties);

        MaxLinesSlider.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Choose how many lines to expand for large descriptions", properties);

        ColorBar.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Select an accent color", properties);

        ResetColorButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Reset the accent color to the application default accent color", properties);

        SystemColorButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Reset the accent color to your system accent color", properties);

        PromptPasswordButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Require a password for confirmation windows", properties);
    }
}