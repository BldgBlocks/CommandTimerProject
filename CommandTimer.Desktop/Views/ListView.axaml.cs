using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.Utilities.ExtensionMethods;
using CommandTimer.Core.ViewModels.MenuItems;
using CommandTimer.Desktop.Views.Menus;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Views;

public partial class ListView : UserControl {

    private readonly List<(Control Control, EventHandler<PointerEventArgs> Handler)> _toolTipPointerEnteredHandlers = [];
    private EventHandler<PointerPressedEventArgs>? _toolTipPointerPressedHandler;

    public ListView() {
        InitializeComponent();

        var listViewData = ServiceProvider.Get<ISerializer>().Deserialize<ListViewData>(Settings.Keys.ListView, Settings.DEFAULT_DATA_FILE)
            ?? new ListViewData();
        DataContext = new ListViewModel(listViewData);
    }

    private static ILibraryManager LibraryManager => ServiceProvider.Get<ILibraryManager>();

    //...

    private bool isNotOverlapping;
    private Rect boundsThatMove = new();
    protected override void OnSizeChanged(SizeChangedEventArgs e) {
        base.OnSizeChanged(e);

        SizeChanged_AppTag();
    }

    private void SizeChanged_AppTag() {
        if (isNotOverlapping) boundsThatMove = AppTag.Bounds;
        isNotOverlapping = !VisualHelpers.IsOverlapping(boundsThatMove, AddLibraryButton.Bounds)
            && !VisualHelpers.IsOverlapping(boundsThatMove, LibraryButton.Bounds);

        AppTag.Padding = new Thickness(isNotOverlapping ? 0 : -200, 0, 0, 0);
        AppTag.Opacity = isNotOverlapping ? 1 : 0;
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        if (DataContext is not ListViewModel viewModel) return;

        /// ToolTips
        if (Design.IsDesignMode is false) {
            SetupCustomToolTips();
        }

        /// Subscriptions
        viewModel.ActiveLibrary = LibraryManager.CurrentLibrary;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        LibraryManager.CurrentLibraryChanging += LibraryManager_CurrentLibraryChanging;
        LibraryManager.CurrentLibraryChanged += LibraryManager_CurrentLibraryChanged;
        Settings.AccentColorSelection.ValueChanged += AccentColorSelection_ValueChanged;
        Settings.ShouldStripeList.ValueChanged += ShouldStripeList_ValueChanged;
        if (Application.Current is not null) {
            Application.Current.ActualThemeVariantChanged += EventHandler_ActualThemeVariantChanged;
        }

        /// Library Selection
        LibraryManager.LibraryAdded += LibraryManager_LibraryNamesChanged;
        LibraryManager.LibraryRemoved += LibraryManager_LibraryNamesChanged;


        ///
        MeasureLibrarySelection();
        MeasureQuickFilterSelection();
        BulkActionsButton_SetupMenuItemsContextMenu();

        AppTag.Transitions = [
            new ThicknessTransition() {
            Duration = TimeSpan.FromMilliseconds(600),
            Easing = new LinearEasing(),
            Property = PaddingProperty,
            },
            new DoubleTransition() {
                Duration = TimeSpan.FromMilliseconds(300),
                Easing = new LinearEasing(),
                Property = OpacityProperty,
            }
        ];

        LibraryButton.ContextMenu = new ContextMenu() {
            ItemsSource = new List<MenuItem>() {
                new() { Header = "Rename", Command = new RelayCommand(Tapped_RenameLibraryButton) }
            }
        };

        /// New Library Flyout
        NewLibraryName.OnAccept += () => {
            if (NewLibraryName.Parameter is string parameter) {
                if (parameter == "rename") {
                    var clone = LibraryManager.RenameLibrary(viewModel.ActiveLibrary, NewLibraryName.Text);
                    LibraryManager.SetCurrent(clone.LibraryName);
                }
                else if (parameter == "new") {
                    LibraryManager.SetCurrent(NewLibraryName.Text);
                }
                else {
                    LibraryManager.SetCurrent(NewLibraryName.Text);
                }
            }
            AddLibraryButton.Flyout?.Hide();
        };
        NewLibraryName.OnCancel += () => {
            AddLibraryButton.Flyout?.Hide();
        };
        AddLibraryButton.Flyout!.Opened += (o, a) => {
            NewLibraryName.BeginEdit();
            NewLibraryName.BackgroundBorder!.BorderThickness = new Thickness(1);
            NewLibraryName.BackgroundBorder.BorderBrush = ServiceProvider.Get<IColorProvider>().ApplicationBrush_Accent.Value.AsBrush();
        };

        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);

        ClearCustomToolTipHandlers();
        LibraryManager.CurrentLibraryChanging -= LibraryManager_CurrentLibraryChanging;
        LibraryManager.CurrentLibraryChanged -= LibraryManager_CurrentLibraryChanged;
        Settings.AccentColorSelection.ValueChanged -= AccentColorSelection_ValueChanged;
        Settings.ShouldStripeList.ValueChanged -= ShouldStripeList_ValueChanged;
        LibraryManager.LibraryAdded -= LibraryManager_LibraryNamesChanged;
        LibraryManager.LibraryRemoved -= LibraryManager_LibraryNamesChanged;
        if (Application.Current is not null) {
            Application.Current.ActualThemeVariantChanged -= EventHandler_ActualThemeVariantChanged;
        }
    }

    private void BulkActionsButton_SetupMenuItemsContextMenu() {
        if (DataContext is not ListViewModel viewModel) return;
        if (TopLevel.GetTopLevel(this) is not MainWindow window) return;

        BulkActionButton.Flyout = new MenuFlyout() {
            ItemsSource = new ListViewMenuItems_BulkActions(viewModel, window.MainWindowLayout).Items
        };
    }

    private void EventHandler_ActualThemeVariantChanged(object? sender, EventArgs e) {
        RestripeList(Settings.ShouldStripeList.Value);
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args) {
        if (DataContext is not ListViewModel viewModel) return;

        if (args.PropertyName == nameof(ListViewModel.LibrarySelection)) {
            MeasureLibrarySelection();
            UpdateControl_MenuItem(viewModel.LibrarySelections, viewModel.LibrarySelection);
        }
        if (args.PropertyName == nameof(ListViewModel.QuickFilter)) {
            MeasureQuickFilterSelection();
            UpdateControl_MenuItem(viewModel.QuickFilters, viewModel.QuickFilter);
        }
    }

    private void LibraryManager_LibraryNamesChanged(object? sender, EventArgs e) {
        if (DataContext is not ListViewModel viewModel) return;

        viewModel.ReloadLibrarySelections();
        UpdateControl_MenuItem(viewModel.LibrarySelections, viewModel.LibrarySelection);
    }

    private void AccentColorSelection_ValueChanged(AppColor color) {
        RestripeList(Settings.ShouldStripeList.Value);
    }

    private void ShouldStripeList_ValueChanged(bool state) => RestripeList(state);

    private void RestripeList(bool state) {
        if (DataContext is not ListViewModel viewModel) return;

        var list = viewModel.RelevantCommandTimers;
        if (state) {
            var length = list.Count;
            for (int i = 0; i < length; i++) {
                list[i].IndexBackgroundStripe(i);
            }
        }
        else list.ForEach(t => t.ResetBackground());
    }

    private void MeasureLibrarySelection() {
        if (DataContext is not ListViewModel viewModel) return;

        LibrarySelectionText.FontSize = VisualHelpers.FindOptimalTextSize((int)ResourceHelper.GetResourceOrThrow<double>("ApplicationFontSize_SubHeader"), 10, viewModel.LibrarySelection.Header, LibrarySelectionText, LibraryButton);
    }

    private void MeasureQuickFilterSelection() {
        if (DataContext is not ListViewModel viewModel) return;

        QuickFilterSelectionText.FontSize = VisualHelpers.FindOptimalTextSize((int)ResourceHelper.GetResourceOrThrow<double>("ApplicationFontSize_SubHeader"), 10, viewModel.QuickFilter.Header, QuickFilterSelectionText, QuickFilterValue);
    }

    private void LibraryManager_CurrentLibraryChanging(object? sender, CommandTimerLibrary e) {
    }

    private void LibraryManager_CurrentLibraryChanged(object? sender, CommandTimerLibrary e) {
        if (DataContext is not ListViewModel viewModel) return;

        viewModel.ActiveLibrary = LibraryManager.CurrentLibrary;
    }

    /// <summary>
    /// Keep menu items in sync as a whole.
    /// </summary>
    private static void UpdateControl_MenuItem(IEnumerable<MenuItemViewModel> menuItems, MenuItemViewModel selected) {
        var colorProvider = ServiceProvider.Get<IColorProvider>();
        var selectedBackground = colorProvider.ApplicationBrush_Accent.Value.AsBrush();
        var selectedForeground = ColorUtilities.GetSlidingContrastColor(colorProvider.ApplicationBrush_Accent.Value).AsBrush();
        var unselectedForeground = colorProvider.ApplicationBrush_Text.Value.AsBrush();
        var selectedFontWeight = ResourceHelper.GetResourceOrThrow<FontWeight>("ApplicationFontWeight_Heavy");
        var unselectedFontWeight = ResourceHelper.GetResourceOrThrow<FontWeight>("ApplicationFontWeight_Medium");

        foreach (var item in menuItems) {
            item.IsSelected = item == selected;
            item.BackgroundColor = item.IsSelected ? selectedBackground : colorProvider.ApplicationBrush_Transparent.Value.AsBrush();
            item.ForegroundColor = item.IsSelected ? selectedForeground : unselectedForeground;
            item.FontWeight = item.IsSelected ? selectedFontWeight : unselectedFontWeight;
        }
    }


    //...

    private CancellationTokenSource? _cts;
    private string _lastUserValue = string.Empty;

    private async void TextChanged_UserSearchBox(object sender, TextChangedEventArgs args) {
        if (DataContext is not ListViewModel viewModel) return;

        if (viewModel.UserSearchEntry == _lastUserValue) return;

        try {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            await Task.Delay(300, _cts.Token);

            _lastUserValue = viewModel.UserSearchEntry;
            viewModel.SortRelevantTimers();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(TextChanged_UserSearchBox)} has thrown the following.", ex);
        }
    }

    private void Tapped_RefreshList(object sender, TappedEventArgs args) {
        if (DataContext is not ListViewModel viewModel) return;

        viewModel.SortRelevantTimers(true);
    }

    private void Tapped_Settings(object sender, TappedEventArgs args)
        => FlyoutBase.ShowAttachedFlyout(SettingsButton);

    private void Tapped_About(object sender, TappedEventArgs args)
        => FlyoutBase.ShowAttachedFlyout(AboutButton);

    private void Tapped_AddNewTimer(object sender, RoutedEventArgs args) {
        if (DataContext is not ListViewModel viewModel) return;

        viewModel.AddNewTimer();
        if (Settings.ShouldStripeList.Value) {
            RestripeList(true);
        }
    }

    private void Tapped_NewLibraryButton(object sender, TappedEventArgs args) {
        NewLibraryName.Parameter = "new";
        LibraryLabel.Text = "Create New Library:";
        NewLibraryName.Text = string.Empty;
        AddLibraryButton.Flyout!.ShowAt(AddLibraryButton);
    }


    private void Tapped_RenameLibraryButton() {
        NewLibraryName.Parameter = "rename";
        LibraryLabel.Text = "Rename Library:";
        NewLibraryName.Text = LibraryManager.CurrentLibrary.LibraryName;
        AddLibraryButton.Flyout!.ShowAt(LibraryButton);
    }

    private void Tapped_UserSearchBox(object sender, TappedEventArgs args) { }

    private void Tapped_DonationButton(object sender, TappedEventArgs args) { }

    private void Tapped_QuickFilterAlt(object sender, TappedEventArgs args) {
        Tapped_QuickFilterSelection(sender, args);

        /// Manually show Button.Flyout
        QuickFilterValue?.Flyout?.ShowAt(QuickFilterValue);
    }
    private void Tapped_QuickFilterSelection(object sender, TappedEventArgs args) {
        if (DataContext is not ListViewModel viewModel) return;

        UpdateControl_MenuItem(viewModel.QuickFilters, viewModel.QuickFilter);
    }

    private void Tapped_SortStrategySelection(object sender, TappedEventArgs args) {
        if (DataContext is not ListViewModel viewModel) return;

        UpdateControl_MenuItem(viewModel.SortStrategies, viewModel.SortStrategy);
        UpdateControl_MenuItem(viewModel.SortDirections, viewModel.SortDirection);
        viewModel.SortOptions.Clear();
        viewModel.SortOptions.AddRange(viewModel.SortStrategies);
        viewModel.SortOptions.AddRange(viewModel.SortDirections);
    }

    private void Tapped_LibrarySelection(object sender, TappedEventArgs args) {
        if (DataContext is not ListViewModel viewModel) return;

        viewModel.ReloadLibrarySelections();
        UpdateControl_MenuItem(viewModel.LibrarySelections, viewModel.LibrarySelection);
    }

    private void Tapped_LogoButton(object? sender, TappedEventArgs args) {

    }


    private void SetupCustomToolTips() {
        ClearCustomToolTipHandlers();

        var properties = ToolTipDefaults.GetDefaults();
        properties.Reference.Placement = PlacementMode.Bottom;
        properties.Reference.VerticalOffset = 5;
        var tooltip = ServiceProvider.Get<IShowToolTip>();

        if (FlyoutBase.GetAttachedFlyout(SettingsButton) is Flyout flyout) {
            flyout.Opening += (o, e) => ServiceProvider.Get<IShowToolTip>().Hide();
        }

        _toolTipPointerPressedHandler = (o, a) => tooltip.Hide();
        NewLibraryName.AddHandler(PointerPressedEvent, _toolTipPointerPressedHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);

        RegisterToolTip(LogoButton, "A place to organize common commands and timed tasks");
        RegisterToolTip(AddLibraryButton, "Add new library");
        RegisterToolTip(NewLibraryName, "Enter a unique name");
        RegisterToolTip(LibraryButton, $"Organize timers into libraries{Environment.NewLine}Right Click: Rename");
        RegisterToolTip(AddTimerButton, "Add new command timer");
        RegisterToolTip(UserSearchBox, "Type a filter");
        RegisterToolTip(QuickFilterButton, "Quick Filter Presets");
        RegisterToolTip(QuickFilterValue, "Selected Quick Filter");
        RegisterToolTip(SortStrategyButton, "Sorting Strategy");
        RegisterToolTip(RefreshButton, "Sort the list again");
        RegisterToolTip(SettingsButton, "Settings");
        RegisterToolTip(AboutButton, "About");
        RegisterToolTip(BulkActionButton, "Perform bulk actions");

        void RegisterToolTip(Control control, string message) {
            EventHandler<PointerEventArgs> handler = (s, args) => tooltip.OnPointerOver(control, message, properties);
            control.PointerEntered += handler;
            _toolTipPointerEnteredHandlers.Add((control, handler));
        }
    }

    private void ClearCustomToolTipHandlers() {
        if (_toolTipPointerPressedHandler is not null) {
            NewLibraryName.RemoveHandler(PointerPressedEvent, _toolTipPointerPressedHandler);
            _toolTipPointerPressedHandler = null;
        }

        foreach (var (control, handler) in _toolTipPointerEnteredHandlers) {
            control.PointerEntered -= handler;
        }

        _toolTipPointerEnteredHandlers.Clear();
    }
}