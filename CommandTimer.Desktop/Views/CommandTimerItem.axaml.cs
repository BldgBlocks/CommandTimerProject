using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using CommandTimer.Core;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.ViewModels;
using CommandTimer.Core.ViewModels.MenuItems;
using CommandTimer.Desktop.Utilities;
using CommunityToolkit.Mvvm.Input;
using ControlsLibrary;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Views;

public partial class CommandTimerItem : UserControl {

    //... Fields

    private NumericUpDownBinding? _secondsFlyoutBinding;
    private NumericUpDownBinding? _daysFlyoutBinding;
    private FlyoutBase? _colorBarFlyout;
    private DoubleTransition? _fadeInTransition;
    private ThicknessTransition? _slideInTransition;
    private BindingExpressionBase? _slideInTransitionToken;
    private BindingExpressionBase? _fadeInTransitionToken;

    private readonly Styles _accentStyle;


    //... Constructor

    public CommandTimerItem() {
        InitializeComponent();

        _accentStyle = LoadAccentStyle();

        /// ColorBar
        var colorBarOnHover = new ShowEditableOnHover(ColorBarHit);
        colorBarOnHover.Connect();

        /// Animation Presets - Avoid rendering issues by setting early in the constructor.
        if (Core.Settings.ShouldAnimate.Value is Core.Settings.AnimationChoice.All) {
            this.Opacity = 0;
            this.Padding = new Thickness(1400, 0, 0, 0);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        viewModel.Opacity = 0;
        viewModel.PaddingLeft = new Thickness(1400, 0, 0, 0);

        /// Get Controls
        var secondsFlyout = FlyoutBase.GetAttachedFlyout(SecondsButton) ?? throw new Core.Exceptions.ControlNotFoundException($"A flyout child control of ${SecondsButton} was not found.");
        _secondsFlyoutBinding = SecondsSelectionView.Bind(secondsFlyout, SecondsButton);
        _secondsFlyoutBinding.Accept += Seconds_OnAccept;

        var daysFlyout = FlyoutBase.GetAttachedFlyout(DaysButton) ?? throw new Core.Exceptions.ControlNotFoundException($"A flyout child control of ${DaysButton} was not found.");
        _daysFlyoutBinding = DaysSelectionView.Bind(daysFlyout, DaysButton);
        _daysFlyoutBinding.Accept += Days_OnAccept;

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

        TimeStrategySelection.Flyout!.Opened += TimeStrategyMenuOpened;

        /// ToolTips
        if (Design.IsDesignMode is false) {
            SetupCustomToolTips();
        }

        /// Color Bar Context
        SetupCopyPasteColorBarContextMenu();

        /// Subscriptions 

        UpdateControl_LibrarySelections();
        LibraryManager.LibraryAdded += LibraryManager_LibraryNamesChanged;
        LibraryManager.LibraryRemoved += LibraryManager_LibraryNamesChanged;

        viewModel.PropertyChanged += ViewModel_PropertyChanged;

        /// Only trigger value changed once value is accepted, not as typing.
        NameBlock.OnAccept += KeyHandler_AcceptName;
        DescriptionBlock.OnAccept += KeyHandler_AcceptDescription;
        CommandBlock.OnAccept += KeyHandler_AcceptCommand;

        /// Global Setting
        Core.Settings.ShouldAnimate.ValueChanged += GlobalSetting_ShouldAnimate;
        if (Core.Settings.ShouldAnimate.Value is Core.Settings.AnimationChoice.All) {
            BindAnimations();
        }
        Application.Current!.ActualThemeVariantChanged += EventHandler_ActualThemeVariantChanged;
        Core.Settings.ShouldExecuteOnTimer.ValueChanged += GlobalSetting_ShouldExecuteOnTimer;
        Core.Settings.MaxLines.ValueChanged += GlobalSetting_MaxLines;
        Core.Settings.AccentColorSelection.ValueChanged += GlobalSetting_AccentColorSelectionChanged;
        Core.Settings.ShouldExpandColorBar.ValueChanged += GlobalSetting_ExpandColorBar;

        /// Finish
        GlobalSetting_MaxLines(Core.Settings.MaxLines.Value);
        GlobalSetting_ExpandColorBar(Core.Settings.ShouldExpandColorBar.Value);
        UpdateUI();

        base.OnLoaded(e);
    }

    private void SetupCopyPasteColorBarContextMenu() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        var contextMenu = new ContextMenu {
            Background = Core.Colors.ApplicationBrush_Background
        };
        var copyItem = new MenuItem() { Header = "Copy" };
        var pasteItem = new MenuItem() { Header = "Paste" };
        contextMenu.Items.Add(copyItem);
        contextMenu.Items.Add(pasteItem);
        copyItem.Click += (s, e) => {
            App.CopyToClipboard(ColorBar.Background?.ToString()!);
            if (FlyoutBase.GetAttachedFlyout(CopyButton) is Flyout flyout) {
                CopyFlyoutTextBlock.Text = "Copied!";
                flyout.ShowAt(ColorBar);
            }
        };
        pasteItem.Click += async (s, e) => {
            if (await App.CopyFromClipboardAsync<string>() is string cast) {
                try {
                    var color = Core.Colors.ParseHexToColor(cast);
                    viewModel.ColorBarColor = new SolidColorBrush(color);
                    UpdateControl_AccentColors();
                }
                catch (FormatException) {
                    if (FlyoutBase.GetAttachedFlyout(CopyButton) is Flyout flyout) {
                        CopyFlyoutTextBlock.Text = $"Provide 'RRGGBB' or 'AARRGGBB' format{Environment.NewLine}Leading '0x' and '#' ok";
                        flyout.ShowAt(ColorBar);
                    }
                    return;
                }
            }
        };
        ColorBarHit.ContextMenu = contextMenu;
    }

    private void TimeStrategyMenuOpened(object? sender, EventArgs e) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        if (TimeStrategySelection.Flyout is MenuFlyout flyout) {
            flyout.Items.ForEach(menuFlyoutItem => {
                if (menuFlyoutItem is MenuItem menuItem) {
                    if (string.Equals(menuItem.Header?.ToString(), viewModel.TimeMode.ToString(), StringComparison.OrdinalIgnoreCase)) {
                        menuItem.Background = Core.Colors.ApplicationBrush_Accent;
                    }
                    else menuItem.Background = Brushes.Transparent;
                }
            });
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        viewModel.PropertyChanged -= ViewModel_PropertyChanged;

        LibraryManager.LibraryAdded -= LibraryManager_LibraryNamesChanged;
        LibraryManager.LibraryRemoved -= LibraryManager_LibraryNamesChanged;

        ColorPickerControl.KeyDown -= ColorBar_KeyDown;
        NameBlock.OnAccept -= KeyHandler_AcceptName;
        DescriptionBlock.OnAccept -= KeyHandler_AcceptDescription;
        CommandBlock.OnAccept -= KeyHandler_AcceptCommand;
        _secondsFlyoutBinding!.Accept -= Seconds_OnAccept;
        _daysFlyoutBinding!.Accept -= Days_OnAccept;
        TimeStrategySelection.Flyout!.Opened -= TimeStrategyMenuOpened;

        /// Global Setting
        if (Application.Current is not null) {
            Application.Current.ActualThemeVariantChanged -= EventHandler_ActualThemeVariantChanged;
        }
        Core.Settings.ShouldAnimate.ValueChanged -= GlobalSetting_ShouldAnimate;
        Core.Settings.ShouldExecuteOnTimer.ValueChanged -= GlobalSetting_ShouldExecuteOnTimer;
        Core.Settings.MaxLines.ValueChanged -= GlobalSetting_MaxLines;
        Core.Settings.AccentColorSelection.ValueChanged -= GlobalSetting_AccentColorSelectionChanged;
        Core.Settings.ShouldExpandColorBar.ValueChanged -= GlobalSetting_ExpandColorBar;

        UnbindAnimations();

        base.OnUnloaded(e);
    }

    private Styles LoadAccentStyle() {
        var assemblyPath = "avares://CommandTimer.Core";
        var styleResourcePath = "avares://CommandTimer.Core/Assets/Styles/CommandTimerItemAccentStyle.axaml";
        try {
            Styles styles;
            /// Test if it was loaded actually
            try {
                styles = App.LoadStyleResource(assemblyPath, styleResourcePath);
                this.Styles.Add(styles);
            }
            catch (Exception ex) {
                throw new ArgumentException($"Style not loaded at path {styleResourcePath}", ex);
            }
            this.Styles.Remove(styles);
            return styles;
        }
        catch (Exception) {
            throw;
        }
    }

    private void GlobalSetting_AccentColorSelectionChanged(SolidColorBrush brush) {
        if (DataContext is not CommandTimerViewModel viewModel) return;
        if (Core.Settings.ShouldExpandColorBar.Value) return;

        viewModel.Accent = brush;
        UpdateUI();
    }

    private void UpdateControl_DatePickerControl() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        var isDate = viewModel.TimeMode is CommandTimerViewModel.TimeModeChoice.Date;
        DatePickerControl.IsVisible = isDate;
        DaysButton.IsVisible = !isDate;
        LoopToggleStackPanel.IsVisible = !isDate;
    }

    private void LibraryManager_LibraryNamesChanged(object? sender, EventArgs e) => UpdateControl_LibrarySelections();
    private void UpdateControl_LibrarySelections() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        viewModel.CopyMoveSelections.Clear();

        var emptyCommand = new RelayCommand<object?>(empty => { });
        var copyCommand = new RelayCommand<object?>(Command_CopyTimerTo);
        var moveCommand = new RelayCommand<object?>(Command_MoveTimerTo);

        List<MenuItemViewModel> copySubMenu = LibraryManager.LibrariesByName
            .Select(libraryName => new MenuItemViewModel(libraryName, [], copyCommand))
            .ToList();
        List<MenuItemViewModel> moveSubMenu = LibraryManager.LibrariesByName
            .Where(libraryName => libraryName != viewModel.LibraryName)
            .Select(libraryName => new MenuItemViewModel(libraryName, [], moveCommand))
            .ToList();

        var copyMenu = new MenuItemViewModel("Copy To", copySubMenu, emptyCommand);
        var moveMenu = new MenuItemViewModel("Move To", moveSubMenu, emptyCommand);

        viewModel.CopyMoveSelections.AddRange([copyMenu, moveMenu]);
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        if (args.PropertyName is nameof(viewModel.IsFavorite)) {
            UpdateControl_Favorite();
        }
        else if (args.PropertyName is nameof(viewModel.IsActive)) {
            UpdateControl_StartStop();
            UpdateControl_Active();
        }
        else if (args.PropertyName is nameof(viewModel.IsPromptForExecute)) {
            UpdateControl_PromptForExecute();
        }
        else if (args.PropertyName is nameof(viewModel.LibraryName)) {
            UpdateControl_LibrarySelections();
        }
        else if (args.PropertyName is nameof(viewModel.TimeMode)) {
            UpdateControl_DatePickerControl();
        }
    }


    //...

    private void GlobalSetting_ShouldExecuteOnTimer(bool value)
        => UpdateControl_StartStop();

    public void GlobalSetting_ShouldAnimate(Core.Settings.AnimationChoice choice) {
        switch (choice) {
            case Core.Settings.AnimationChoice.None:
                UnbindAnimations();
                UpdateUI();
                break;
            case Core.Settings.AnimationChoice.All:
                BindAnimations();
                UpdateUI();
                break;
            default:
                break;
        }
    }

    private void GlobalSetting_MaxLines(int value) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        viewModel.MaxLines = value;
    }

    private void GlobalSetting_ExpandColorBar(bool value)
        => UpdateControl_AccentColors();

    private void SetAccentToColorBar(CommandTimerViewModel viewModel) {
        if (this.Styles.Contains(_accentStyle) is false) {
            this.Styles.Add(_accentStyle);
        }
        viewModel.Accent = viewModel.ColorBarColor;
    }

    private void SetAccentToGlobal(CommandTimerViewModel viewModel) {
        this.Styles.Remove(_accentStyle);
        viewModel.Accent = Core.Colors.ApplicationBrush_Accent;
    }

    public async void BindAnimations() {
        /// Animation Preset
        this.Opacity = 0;
        this.Padding = new Thickness(1400, 0, 0, 0);

        _fadeInTransition = DoubleTransitionHelper
            .CreateAnimation()
            .WithProperty(Visual.OpacityProperty)
            .WithTime(400)
            .WithEasing(new LinearEasing())
            .WithBind(this, nameof(CommandTimerViewModel.Opacity), out _fadeInTransitionToken);

        _slideInTransition = ThicknessTransitionHelper
            .CreateAnimation()
            .WithProperty(PaddingProperty)
            .WithTime(600)
            .WithEasing(new CircularEaseInOut())
            .WithBind(this, nameof(CommandTimerViewModel.PaddingLeft), out _slideInTransitionToken);

        try {
            /// Start the transition on the next frame.
            await Dispatcher.UIThread.InvokeAsync(async () => {
                if (DataContext is CommandTimerViewModel vm) {
                    vm.Opacity = 0;
                    vm.PaddingLeft = new Thickness(1400, 0, 0, 0);

                    await Task.Delay(400);

                    vm.Opacity = 1;
                    vm.PaddingLeft = new Thickness(0, 0, 0, 0);
                }
            }, DispatcherPriority.Background);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(BindAnimations)} has thrown the following.", ex);
        }
    }

    public void UnbindAnimations() {
        if (_fadeInTransitionToken is null) return;
        if (_slideInTransitionToken is null) return;
        _fadeInTransition?.Remove(this);
        _slideInTransition?.Remove(this);
        _fadeInTransition?.Unbind(_fadeInTransitionToken);
        _slideInTransition?.Unbind(_slideInTransitionToken);
        _fadeInTransition = null;
        _slideInTransition = null;
    }


    //...

    private void EventHandler_ActualThemeVariantChanged(object? sender, EventArgs e) => UpdateUI();

    /// <summary>
    /// Button displayed uses a OneWay binding so the value must be accepted, prevents background updates and serialization unnecessarily.
    /// Also, the timespan is set in a split manner to respect seconds, so the display is kept in sync, then the value is inserted to the target time span.
    /// </summary>
    private void Seconds_OnAccept() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        var existing = viewModel.TargetTimeSpanTillExecution;
        viewModel.TargetTimeSpanTillExecution = new TimeSpan(existing.Days, existing.Hours, existing.Minutes, (int)(SecondsSelectionView.NumericUpDownControl.Value ?? 0));
    }

    private void Days_OnAccept() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        var existing = viewModel.TargetTimeSpanTillExecution;
        viewModel.TargetTimeSpanTillExecution = new TimeSpan((int)(DaysSelectionView.NumericUpDownControl.Value ?? 0), existing.Hours, existing.Minutes, existing.Seconds);
    }

    public void KeyHandler_AcceptName() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        viewModel.Library.ChangeTimerNameTo(viewModel, NameBlock.Text);
    }

    public void KeyHandler_AcceptDescription() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        viewModel.Description = DescriptionBlock.Text ?? string.Empty;
    }

    public void KeyHandler_AcceptCommand() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        viewModel.Command = CommandBlock.Text ?? string.Empty;
    }

    private void UpdateUI() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        UpdateControl_Text();
        UpdateControl_Favorite();
        UpdateControl_StartStop();
        UpdateControl_Active();
        UpdateControl_PromptForExecute();
        UpdateControl_DatePickerControl();

        viewModel.Update_Countdown();
    }

    private void UpdateControl_AccentColors() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        /// Accent Theming
        if (Core.Settings.ShouldExpandColorBar.Value) {
            SetAccentToColorBar(viewModel);
        }
        else {
            SetAccentToGlobal(viewModel);
        }

        /// Command Block
        Color highlight = viewModel.Accent.Color;
        CommandBlock.Background = new LinearGradientBrush {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops = {
                new GradientStop { Color = new Color(100, highlight.R, highlight.G, highlight.B), Offset = 0 },
                new GradientStop { Color = Avalonia.Media.Colors.Transparent, Offset = 1 }
            }
        };
    }

    private void UpdateControl_Text() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        /// Text Colors - CustomTextBox Immutable brush must be set in code-behind.
        var textColor = Core.Colors.ApplicationBrush_Text;
        NameBlock.Foreground = textColor;
        DescriptionBlock.Foreground = textColor;
        CommandBlock.Foreground = textColor;
    }

    private void UpdateControl_Active() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        viewModel.Update_Countdown();
        if (viewModel.IsActive is true) {
            ActiveIndicatorIcon.Foreground = Core.Colors.ApplicationBrush_DoThingIntended;
            CountdownBlock.Foreground = Core.Colors.ApplicationBrush_DoThingIntended;
            CountdownBlock.FontWeight = FontWeight.SemiBold;
        }
        else {
            ActiveIndicatorIcon.Foreground = Core.Colors.ApplicationBrush_Inconspicuous;
            CountdownBlock.Foreground = Core.Colors.ApplicationBrush_Inconspicuous;
            CountdownBlock.FontWeight = FontWeight.Light;
        }
    }

    private void UpdateControl_StartStop() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        if (viewModel.IsActive) {
            StartStopIcon.Height = 27;
            StartStopIcon.Width = 27;
            StartStopIcon.Kind = MaterialIconKind.Stop;
            StartStopIcon.Foreground = Core.Colors.ApplicationBrush_Bad;
            StartStopIcon.IsHitTestVisible = true;
            StartStopButton.IsHitTestVisible = true;
        }
        else {
            if (Core.Settings.ShouldExecuteOnTimer.Value) {
                StartStopIcon.Height = 27;
                StartStopIcon.Width = 27;
                StartStopIcon.Kind = MaterialIconKind.Play;
                StartStopIcon.Foreground = Core.Colors.ApplicationBrush_DoThingIntended;
                StartStopIcon.IsHitTestVisible = true;
                StartStopButton.IsHitTestVisible = true;
            }
            else {
                StartStopIcon.Height = 20;
                StartStopIcon.Width = 20;
                StartStopIcon.Kind = MaterialIconKind.No;
                StartStopIcon.Foreground = Core.Colors.ApplicationBrush_Bad;
                StartStopIcon.IsHitTestVisible = false;
                StartStopButton.IsHitTestVisible = false;
            }
        }
    }

    private void UpdateControl_Favorite() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        FavoriteIcon.Foreground = viewModel.IsFavorite ? Core.Colors.ApplicationBrush_Highlight : Core.Colors.ApplicationBrush_Contrast;
    }

    private void UpdateControl_PromptForExecute() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        PromptExecuteIcon.Foreground = viewModel.IsPromptForExecute ? Brushes.Red : Core.Colors.ApplicationBrush_Inconspicuous;
    }


    //... Menu Commands

    private async void Command_CopyTimerTo(object? parameter) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        if (parameter is MenuItemViewModel menuItem && LibraryManager.FindLibrary(menuItem.Header) is CommandTimerLibrary otherLibrary) {
            if (viewModel.Library == otherLibrary) {
                otherLibrary.AddToLibrary(CommandTimerViewModel.Clone(viewModel));
                return;
            }
            if (otherLibrary.ContainsName(viewModel.Name)) {
                if (TopLevel.GetTopLevel(this) is not MainWindow window) return;

                try {
                    bool? result = await ConfirmationDialog.Create()
                                                           .WithMessage($"Replace existing? {viewModel.Name}")
                                                           .Show(window.MainWindowLayout);
                    if (result is false) return;
                    else if (result is true) {
                        otherLibrary.RemoveFromLibrary(otherLibrary.GetSingleTimerInstance(viewModel.Name)!);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) {
                    throw new Exception($"Method: {nameof(Command_CopyTimerTo)} has thrown the following.", ex);
                }
            }
            otherLibrary.AddToLibrary(CommandTimerViewModel.Clone(viewModel));
        }
    }
    private async void Command_MoveTimerTo(object? parameter) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        if (parameter is MenuItemViewModel menuItem && LibraryManager.FindLibrary(menuItem.Header) is CommandTimerLibrary otherLibrary) {
            if (viewModel.Library == otherLibrary) return;
            if (otherLibrary.ContainsName(viewModel.Name)) {
                if (TopLevel.GetTopLevel(this) is not MainWindow window) return;

                try {
                    bool? result = await ConfirmationDialog.Create()
                                                           .WithMessage($"Replace existing? {viewModel.Name}")
                                                           .Show(window.MainWindowLayout);
                    if (result is false) return;
                    else if (result is true) {
                        otherLibrary.RemoveFromLibrary(otherLibrary.GetSingleTimerInstance(viewModel.Name)!);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) {
                    throw new Exception($"Method: {nameof(Command_MoveTimerTo)} has thrown the following.", ex);
                }
            }
            viewModel.Library.RemoveFromLibrary(viewModel);
            otherLibrary.AddToLibrary(viewModel);
        }
    }


    //... ColorPicker

    private void Changed_ColorPick(object sender, ColorChangedEventArgs args)
        => Sync_ColorPick();

    private void Sync_ColorPick() {
        ColorApplyButton.Background = new SolidColorBrush(ColorPickerControl.Color);
        ColorApplyButton.Foreground = new SolidColorBrush(Core.Colors.GetSlidingContrastColor(ColorPickerControl.Color));
    }

    private void Tapped_ColorBar(object sender, TappedEventArgs args) {
        Sync_ColorPick();
        var colorBarFlyout = FlyoutBase.GetAttachedFlyout(ColorBar);
        colorBarFlyout?.ShowAt(ColorBar);
    }

    private void ColorBar_KeyDown(object? sender, KeyEventArgs args) {
        if (args.Key == Key.Enter) {
            ColorPick_ApplyColor();
            FlyoutBase.GetAttachedFlyout(ColorBar)?.Hide();
            args.Handled = true;
        }
        if (args.Key == Key.Escape) {
            ColorPick_CancelColor();
            FlyoutBase.GetAttachedFlyout(ColorBar)?.Hide();
            args.Handled = true;
        }
    }

    private void Tapped_ApplyColor(object sender, TappedEventArgs args) {
        ColorPick_ApplyColor();
        FlyoutBase.GetAttachedFlyout(ColorBar)?.Hide();
    }

    private void ColorPick_ApplyColor() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        viewModel.ColorBarColor = new SolidColorBrush(ColorPickerControl.Color);
        UpdateControl_AccentColors();
    }

    private void Tapped_CancelColor(object sender, TappedEventArgs args) {
        ColorPick_CancelColor();
        FlyoutBase.GetAttachedFlyout(ColorBar)?.Hide();
    }

    private void ColorPick_CancelColor() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        ColorPickerControl.Color = viewModel.ColorBarColor.Color;
    }

    private void Tapped_FavoriteItem(object sender, TappedEventArgs args) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        viewModel.IsFavorite = !viewModel.IsFavorite;
    }

    private void Tapped_StartStop(object sender, TappedEventArgs args) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        viewModel.IsActive = !viewModel.IsActive;
    }

    private void Tapped_CopyButton(object sender, TappedEventArgs args) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        if (sender is Button button && FlyoutBase.GetAttachedFlyout(button) is Flyout flyout) {
            flyout.VerticalOffset = -5;
            flyout.HorizontalOffset = -20;

            /// Empty - Changed to always have default...
            if (string.IsNullOrWhiteSpace(viewModel.Command) || viewModel.Command == CommandTimerViewModel.PLACEHOLDER_COMMAND) {
                CopyFlyoutTextBlock.Text = "Empty";
                flyout.ShowAt(button);
                return;
            }

            App.CopyToClipboard(viewModel.Command);

            CopyFlyoutTextBlock.Text = "Copied!";
            flyout.ShowAt(button);
        }
    }

    private async void Tapped_Execute(object sender, TappedEventArgs args) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        try {
            if (viewModel.IsPromptForExecute || Core.Settings.ShouldPromptByDefault.Value) {
                if (TopLevel.GetTopLevel(this) is not MainWindow window) return;

                bool? result = await ConfirmationDialog.Create()
                                                       .WithMessage($"Run command: {viewModel.Name}?")
                                                       .WithPasswordEntry(Core.Settings.ShouldUsePasswordConfirmation.Value)
                                                       .Show(window.MainWindowLayout);
                if (result is false) return;
            }
            Core.MessageRelay.OnMessagePosted(this, $"Executing [{viewModel.Name}]", Core.MessageRelay.MessageCategory.User);

            viewModel.ExecuteCommand();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(Tapped_Execute)} has thrown the following.", ex);
        }
    }

    private async void Tapped_PromptForExecute(object sender, TappedEventArgs args) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        try {
            /// To remove prompt you must confirm via prompt.
            if (viewModel.IsPromptForExecute) {
                if (TopLevel.GetTopLevel(this) is not MainWindow window) return;

                bool? result = await ConfirmationDialog.Create()
                                                       .WithTitle("Disable Confirmation Prompt?")
                                                       .WithPasswordEntry(Core.Settings.ShouldUsePasswordConfirmation.Value)
                                                       .Show(window.MainWindowLayout);
                if (result is false) return;
            }
            viewModel.IsPromptForExecute = !viewModel.IsPromptForExecute;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(Tapped_PromptForExecute)} has thrown the following.", ex);
        }
    }

    private async void Tapped_Remove(object sender, TappedEventArgs args) {
        await RemoveTimer();
    }

    public async Task RemoveTimer() {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        viewModel.PaddingLeft = new Thickness(1400, 0, 0, 0);
        viewModel.Opacity = 0;

        await Task.Delay(300);

        viewModel.Library.RemoveFromLibrary(viewModel);
    }

    private void Tapped_ShouldLog(object sender, TappedEventArgs args) { }

    private void Tapped_ShowTerminal(object sender, TappedEventArgs args) { }

    private void Tapped_Loop(object sender, TappedEventArgs args) { }

    private void Tapped_AutoStart(object sender, TappedEventArgs args) { }

    private void Tapped_CopyMoveLibraryButton(object sender, TappedEventArgs args) { }

    private void SelectedChanged_TimePicker(object sender, TimePickerSelectedValueChangedEventArgs args) {
        if (DataContext is not CommandTimerViewModel viewModel) return;

        if (args.NewTime is not null) {
            viewModel.TargetTimeSpanTillExecution = new TimeSpan(viewModel.TargetTimeSpanTillExecution.Days, args.NewTime.Value.Hours, args.NewTime.Value.Minutes, viewModel.TargetTimeSpanTillExecution.Seconds);
        }
    }

    /// <summary>
    /// Built-in tooltip is not flexible, flicker and crash in linux.
    /// </summary>
    private void SetupCustomToolTips() {
        var properties = ToolTipDefaults.GetDefaults();
        var tooltip = ServiceProvider.Get<IShowToolTip>();

        TimePickerControl.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Choose a time as duration or absolute", properties);

        NameBlock.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Enter a unique name", properties);

        CopyMoveLibrary.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Copy or move to another library", properties);

        DescriptionBlock.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, $"A place for description or notes{Environment.NewLine}Check the settings menu to expand this field", properties);

        CopyButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Copy to Clipboard", properties);

        PromptExecuteButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Prompt for confirmation upon manual execution", properties);

        ExecuteNowButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Run the command right now", properties);

        CommandBlockAndBorderPanel.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, $"Enter the command exactly as in a terminal{Environment.NewLine}Check the Settings menu to expand this field", properties);

        FavoriteButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Favorites are always filtered to the top when relevant", properties);

        RemoveButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Delete", properties);

        TimeStrategySelection.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "'Duration' is a timer, 'Time' is a clock time for execution, 'Date' is a day and time", properties);

        SecondsButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Enter a seconds value.", properties);

        DaysButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Enter a days value.", properties);

        CountdownBlock.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Countdown/Total Time", properties);

        LogStackPanel.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Should activity be logged.", properties);

        ShowTerminalStackPanel.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Open a terminal to show command output", properties);

        LoopToggleStackPanel.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Loop or repeat the timer continuously. Daily for time, or every duration.", properties);

        AutoStartToggleStackPanel.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Start the timer automatically upon program start", properties);

        ActiveIndicator.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Active Status", properties);

        StartStopButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Start/Stop timer", properties);
    }
}