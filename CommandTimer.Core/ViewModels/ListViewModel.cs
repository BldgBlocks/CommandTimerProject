using Avalonia.Threading;
using CommandTimer.Core.Utilities.ExtensionMethods;
using CommandTimer.Core.ViewModels.MenuItems;
using System.Collections.ObjectModel;
using System.Threading;

namespace CommandTimer.Core.ViewModels;

public partial class ListViewModel : ViewModelBase {

    //... Fields

    private const int FULL_ANIMATION_BEFORE_RAMP = 7;
    private CancellationTokenSource _cts = new();
    private LibrarySelectionsMenu _libraryMenu;
    private static ILibraryManager LibraryManager => ServiceProvider.Get<ILibraryManager>();

    //... Constructor

    public ListViewModel() : this(new ListViewData()) { }

    public ListViewModel(ListViewData data) {
        Data = data;

        /// Quick Filters Menu List
        var quickFilterMenu = new QuickFilterMenu(Command_QuickFilterSelection);
        QuickFilters = new ObservableCollection<MenuItemViewModel_CommandTimers>(quickFilterMenu.Items);

        /// Sort Strategy Menu
        var sortStrategyMenu = new SortingStrategiesMenu(Command_SortStrategySelection);
        SortStrategies = new ObservableCollection<MenuItemViewModel_CommandTimers>(sortStrategyMenu.Items);

        /// Sort Direction Menu
        var sortDirectionMenu = new SortingDirectionsMenu(Command_SortDirectionSelection);
        SortDirections = new ObservableCollection<MenuItemViewModel_CommandTimers>(sortDirectionMenu.Items);

        _libraryMenu = new LibrarySelectionsMenu(Command_LibrarySelection, Command_RemoveLibrary);

        /// Global Action
        ActionRelay.ActionPosted += (o, a) => {
            if (a.ActionKey == Settings.Keys.ActionRelay_Serialization) {
                Serialize();
            }
        };

        /// Finish
        if (string.IsNullOrWhiteSpace(Data.LibrarySelection)) {
            Data.LibrarySelection = Settings.Keys.DefaultLibrary;
        }
        _ = QuickFilter;
        _ = SortStrategy;
        _ = SortDirection;
        LibraryManager.SetCurrent(Data.LibrarySelection);
    }

    internal ListViewData Data { get; }

    //... Base Implementation

    public override void Serialize()
        => ServiceProvider.Get<ISerializer>().Serialize(Settings.Keys.ListView, Data, Settings.DEFAULT_DATA_FILE);

    //... Event Handlers

    private void ActiveLibrary_TimerAdded(object? sender, CommandTimerViewModel timer)
        => RelevantCommandTimers.Insert(0, timer);
    private void ActiveLibrary_TimerRemoved(object? sender, CommandTimerViewModel timer)
        => SortRelevantTimers();

    //... Menu Action Bindings

    private void Command_SortStrategySelection(object? parameter)
        => SortStrategy = parameter as MenuItemViewModel_CommandTimers ?? SortStrategy;
    private void Command_SortDirectionSelection(object? parameter)
        => SortDirection = parameter as MenuItemViewModel_CommandTimers ?? SortDirection;
    private void Command_QuickFilterSelection(object? parameter)
        => QuickFilter = parameter as MenuItemViewModel_CommandTimers ?? QuickFilter;
    private void Command_LibrarySelection(object? parameter) {
        LibrarySelection = parameter as MenuItemViewModel ?? LibrarySelection;
    }

    public static async void Command_RemoveLibrary(object? parameter) {
        var libraryName = (parameter as MenuItemViewModel)?.Header ?? string.Empty;
        bool? result = await ServiceProvider.Get<IAskTheUser>()
                                            .Ask("Confirmation",
                                                $"Delete Library: '{libraryName}'{Environment.NewLine}{Environment.NewLine}Do you want to delete backups of this library?",
                                                true, true);
        if (result is null) return;
        if (result is false) LibraryManager.RemoveLibrary(libraryName);
        if (result.Value) LibraryManager.RemoveLibrary(libraryName, true);
    }

    //... Binding Properties

    public ObservableCollection<CommandTimerViewModel> RelevantCommandTimers { get; } = [];
    private string _UserSearchEntry = string.Empty;
    public string UserSearchEntry { get => _UserSearchEntry; set => SetProperty(ref _UserSearchEntry, value); }


    public ObservableCollection<MenuItemViewModel_CommandTimers> QuickFilters { get; }
    public ObservableCollection<MenuItemViewModel_CommandTimers> SortStrategies { get; }
    public ObservableCollection<MenuItemViewModel_CommandTimers> SortDirections { get; }
    public ObservableCollection<MenuItemViewModel_CommandTimers> SortOptions { get; } = [];
    public ObservableCollection<MenuItemViewModel> ThemeSelections { get; } = [];
    public ObservableCollection<MenuItemViewModel> LibrarySelections { get; } = [];

    //...

    private CommandTimerLibrary _ActiveLibrary = new() { LibraryName = Settings.Keys.DefaultLibrary };
    public CommandTimerLibrary ActiveLibrary {
        get => _ActiveLibrary;
        set {
            if (_ActiveLibrary != value) {
                ActiveLibrary.TimerAdded -= ActiveLibrary_TimerAdded;
                ActiveLibrary.TimerRemoved -= ActiveLibrary_TimerRemoved;

                SetProperty(ref _ActiveLibrary, value, Save.No, Notify.Yes);
                // TODO: Move GetMenuItem to each Menu implementation
                LibrarySelection = _libraryMenu.GetMenuItem(value.LibraryName) ?? LibrarySelection;

                ActiveLibrary.TimerAdded += ActiveLibrary_TimerAdded;
                ActiveLibrary.TimerRemoved += ActiveLibrary_TimerRemoved;

                SortRelevantTimers(true);
            }
        }
    }

    public MenuItemViewModel LibrarySelection {
        get {
            var menuItem = _libraryMenu.GetMenuItem(Data.LibrarySelection)
                ?? _libraryMenu.GetMenuItem(Settings.Keys.DefaultLibrary)
                ?? LibrarySelections.FirstOrDefault()
                ?? throw new InvalidOperationException("No library selections are available.");
            Data.LibrarySelection = menuItem.Header;
            return menuItem;
        }

        set {
            if (value is null) return;
            var oldValue = Data.LibrarySelection;
            Data.LibrarySelection = value.Header;
            if (SetPropertySaveNotify(oldValue, value.Header)) {
                LibraryManager.SetCurrent(value.Header);
            }
        }
    }

    //...

    public MenuItemViewModel_CommandTimers QuickFilter {
        get {
            if (GetMenuItem(QuickFilters, Data.QuickFilter) is not MenuItemViewModel_CommandTimers menuItem) {
                menuItem = QuickFilters[0];
                Data.QuickFilter = menuItem.Header;
            }

            return menuItem;
        }

        private set {
            var oldValue = Data.QuickFilter;
            Data.QuickFilter = value.Header;
            if (SetPropertySaveNotify(oldValue, value.Header)) {
                SortRelevantTimers();
            }
        }
    }

    //...

    public MenuItemViewModel_CommandTimers SortStrategy {
        get {
            if (GetMenuItem(SortStrategies, Data.SortStrategy) is not MenuItemViewModel_CommandTimers menuItem) {
                menuItem = SortStrategies[0];
                Data.SortStrategy = menuItem.Header;
            }

            return menuItem;
        }

        private set {
            var oldValue = Data.SortStrategy;
            Data.SortStrategy = value.Header;
            if (SetPropertySaveNotify(oldValue, value.Header)) {
                SortRelevantTimers();
            }
        }
    }

    //...

    public MenuItemViewModel_CommandTimers SortDirection {
        get {
            if (GetMenuItem(SortDirections, Data.SortDirection) is not MenuItemViewModel_CommandTimers menuItem) {
                menuItem = SortDirections[0];
                Data.SortDirection = menuItem.Header;
            }

            return menuItem;
        }

        private set {
            var oldValue = Data.SortDirection;
            Data.SortDirection = value.Header;
            if (SetPropertySaveNotify(oldValue, value.Header)) {
                SortRelevantTimers();
            }
        }
    }

    //... Public Methods

    public static MenuItemViewModel? GetMenuItem(IEnumerable<MenuItemViewModel> menuItems, string itemName)
        => menuItems.FirstOrDefault((item) => item.Header == itemName);

    public void AddNewTimer()
        => ActiveLibrary.AddToLibrary(CommandTimerViewModel.Create());

    public void RemoveTimer(CommandTimerViewModel timer)
        => ActiveLibrary.RemoveFromLibrary(timer);

    public void SortRelevantTimers(bool forceRefresh = false) {
        var list = ActiveLibrary.CommandTimers.ToList();

        list.FuzzySearch(UserSearchEntry);
        QuickFilter?.MenuItemAction(list);
        SortStrategy?.MenuItemAction(list);
        SortDirection?.MenuItemAction(list);
        SetRelevantTimers(list, forceRefresh);
    }

    private async void SetRelevantTimers(IList<CommandTimerViewModel> list, bool forceRefresh = false) {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();

        try {
            int i = 0,
                existingCount = RelevantCommandTimers.Count,
                minCount = Math.Min(existingCount, list.Count),
                maxCount = Math.Max(existingCount, list.Count),
                e = RelevantCommandTimers.Zip(list, (item1, item2) => (item1, item2))
                                         .TakeWhile(x => x.item1.Name == x.item2.Name)
                                         .Count();

            if (!forceRefresh && e == maxCount) return;
            if (forceRefresh) e = 0;

            var delay = Settings.ShouldAnimate.Value is Settings.AnimationChoice.All ? 100 : 50;
            for (; e < list.Count; e++) {
                if (i++ < FULL_ANIMATION_BEFORE_RAMP) {
                    await Task.Delay(delay, _cts.Token);
                    if (e > existingCount - 1) {
                        RelevantCommandTimers.Add(list[e]);
                    }
                    else {
                        RelevantCommandTimers[e] = list[e];
                    }
                    if (Settings.ShouldStripeList.Value) {
                        list[e].IndexBackgroundStripe(e);
                    }
                }
                else {
                    /// Maintain UI responsiveness for large lists.
                    Dispatcher.UIThread.Invoke(() => {
                        if (e > existingCount - 1) {
                            RelevantCommandTimers.Add(list[e]);
                        }
                        else {
                            RelevantCommandTimers[e] = list[e];
                        }
                        if (Settings.ShouldStripeList.Value) {
                            list[e].IndexBackgroundStripe(e);
                        }
                    }, DispatcherPriority.Background, _cts.Token);
                }
            }
            /// Trim the remainder of the list
            if (list.Count < existingCount) {
                RelevantCommandTimers.RemoveRange(list.Count, existingCount - list.Count);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(SetRelevantTimers)} has thrown the following.", ex);
        }
    }

    public void ReloadLibrarySelections() {
        _libraryMenu.ReloadItems(LibrarySelections);
        if (ActiveLibrary.LibraryName != LibrarySelection.Header) {
            LibraryManager.SetCurrent(LibrarySelection.Header);
        }
    }

}

