using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using CommandTimer.Core;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.ViewModels;
using CommandTimer.Core.ViewModels.MenuItems;
using CommandTimer.Desktop.Utilities;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Views;

public partial class ListView : UserControl {

    public ListView() {
        InitializeComponent();

        if (ServiceProvider.Get<ISerializer>().Deserialize<ListViewModel>(Core.Settings.Keys.ListView, Core.Settings.DEFAULT_DATA_FILE) is not ListViewModel listViewModel) {
            listViewModel = new ListViewModel();
        }
        DataContext = listViewModel;
    }


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
        Core.Settings.AccentColorSelection.ValueChanged += AccentColorSelection_ValueChanged;
        Core.Settings.ShouldStripeList.ValueChanged += ShouldStripeList_ValueChanged;
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
                    LibraryManager.LoadLibraryToCurrent(clone.LibraryName);
                }
                else if (parameter == "new") {
                    LibraryManager.LoadLibraryToCurrent(NewLibraryName.Text);
                }
                else {
                    LibraryManager.LoadLibraryToCurrent(NewLibraryName.Text);
                }
            }
            AddLibraryButton.Flyout?.Hide();
        };
        NewLibraryName.OnCancel += () => {
            AddLibraryButton.Flyout?.Hide();
        };
        AddLibraryButton.Flyout!.Opened += (o, a) => {
            NewLibraryName.TextBox!.SelectAll();
            NewLibraryName.TextBox.Focus();
            NewLibraryName.TextBox.CaretIndex = 0;
            NewLibraryName.BackgroundBorder!.BorderThickness = new Thickness(1);
            NewLibraryName.BackgroundBorder.BorderBrush = Core.Colors.ApplicationBrush_Accent;
        };

        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);

        LibraryManager.CurrentLibraryChanging -= LibraryManager_CurrentLibraryChanging;
        LibraryManager.CurrentLibraryChanged -= LibraryManager_CurrentLibraryChanged;
        Core.Settings.AccentColorSelection.ValueChanged -= AccentColorSelection_ValueChanged;
        Core.Settings.ShouldStripeList.ValueChanged -= ShouldStripeList_ValueChanged;
        LibraryManager.LibraryAdded -= LibraryManager_LibraryNamesChanged;
        LibraryManager.LibraryRemoved -= LibraryManager_LibraryNamesChanged;
        if (Application.Current is not null) {
            Application.Current.ActualThemeVariantChanged -= EventHandler_ActualThemeVariantChanged;
        }
    }

    private void BulkActionsButton_SetupMenuItemsContextMenu() {
        if (DataContext is not ListViewModel viewModel) return;
        if (MainWindow.Instance is not MainWindow window) return;

        BulkActionButton.Flyout = new MenuFlyout() { 
            ItemsSource = new ListViewMenuItems_BulkActions(viewModel, window.MainWindowLayout).Items
        };
    }

    private void EventHandler_ActualThemeVariantChanged(object? sender, EventArgs e) {
        RestripeList(Core.Settings.ShouldStripeList.Value);
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

    private void AccentColorSelection_ValueChanged(SolidColorBrush obj) {
        /// Cycle flyout for repaint.
        if (FlyoutBase.GetAttachedFlyout(SettingsButton) is Flyout settingsFlyout && settingsFlyout.IsOpen) {
            settingsFlyout.Hide();
            settingsFlyout.ShowAt(SettingsButton);
        }
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

        LibrarySelectionText.FontSize = VisualHelpers.FindOptimalTextSize((int)Core.Colors.GetSetting<double>("ApplicationFontSize_SubHeader"), 10, viewModel.LibrarySelection.Header, LibrarySelectionText, LibraryButton);
    }

    private void MeasureQuickFilterSelection() {
        if (DataContext is not ListViewModel viewModel) return;

        QuickFilterSelectionText.FontSize = VisualHelpers.FindOptimalTextSize((int)Core.Colors.GetSetting<double>("ApplicationFontSize_SubHeader"), 10, viewModel.QuickFilter.Header, QuickFilterSelectionText, QuickFilterValue);
    }

    private void LibraryManager_CurrentLibraryChanging(object? sender, CommandTimerLibrary e) {
        if (DataContext is not ListViewModel viewModel) return;

        viewModel.ActiveLibrary.TimerRemoved -= ActiveLibrary_TimerRemoved;
    }

    private void LibraryManager_CurrentLibraryChanged(object? sender, CommandTimerLibrary e) {
        if (DataContext is not ListViewModel viewModel) return;

        viewModel.ActiveLibrary = LibraryManager.CurrentLibrary;
        viewModel.ActiveLibrary.TimerRemoved += ActiveLibrary_TimerRemoved;
    }

    private void ActiveLibrary_TimerRemoved(object? sender, CommandTimerViewModel e) {
        if (DataContext is not ListViewModel viewModel) return;

        viewModel.SortRelevantTimers();
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

        viewModel.SortRelevantTimers();
    }

    private void Tapped_Settings(object sender, TappedEventArgs args)
        => FlyoutBase.ShowAttachedFlyout(SettingsButton);

    private void Tapped_About(object sender, TappedEventArgs args)
        => FlyoutBase.ShowAttachedFlyout(AboutButton);

    private void Tapped_AddNewTimer(object sender, RoutedEventArgs args) {
        if (DataContext is not ListViewModel viewModel) return;

        viewModel.AddNewTimer();
        if (Core.Settings.ShouldStripeList.Value) {
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
        var properties = ToolTipDefaults.GetDefaults();
        properties.Reference.Placement = PlacementMode.Bottom;
        properties.Reference.VerticalOffset = 5;
        var tooltip = ServiceProvider.Get<IShowToolTip>();

        if (FlyoutBase.GetAttachedFlyout(SettingsButton) is Flyout flyout) {
            flyout.Opening += (o, e) => ServiceProvider.Get<IShowToolTip>().Hide();
        }

        NewLibraryName.AddHandler(PointerPressedEvent, (o, a) => tooltip.Hide(), RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);

        LogoButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "A place to organize common commands and timed tasks", properties);

        AddLibraryButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Add new library", properties);

        NewLibraryName.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Enter a unique name", properties);

        LibraryButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, $"Organize timers into libraries{Environment.NewLine}Right Click: Rename", properties);

        AddTimerButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Add new command timer", properties);

        UserSearchBox.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Type a filter", properties);

        QuickFilterButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Quick Filter Presets", properties);

        QuickFilterValue.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Selected Quick Filter", properties);

        SortStrategyButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Sorting Strategy", properties);

        RefreshButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Sort the list again", properties);

        SettingsButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Settings", properties);

        AboutButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "About", properties);

        BulkActionButton.PointerEntered += (s, args)
            => tooltip.OnPointerOver((Control)s!, "Perform bulk actions", properties);
    }
}