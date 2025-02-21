using Avalonia;
using Avalonia.Threading;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.ViewModels.MenuItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CommandTimer.Core.ViewModels;

public partial class ListViewModel : ViewModelBase {

    //... Fields

    private const int FULL_ANIMATION_BEFORE_RAMP = 7;
    private CancellationTokenSource _cts = new();
    private LibrarySelectionsMenu _libraryMenu;

    //... Constructor

    public ListViewModel() {

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
        Core.ActionRelay.ActionPosted += (o, a) => {
            if (a.ActionKey == Core.Settings.Keys.ActionRelay_Serialization) {
                Serialize();
            }
        };

        /// Finish
        _QuickFilter = QuickFilter.Header;
        _SortStrategy = SortStrategy.Header;
        _SortDirection = SortDirection.Header;
    }

    //... Base Implementation

    public override void Serialize()
        => ServiceProvider.Get<ISerializer>().Serialize(Core.Settings.Keys.ListView, this, Core.Settings.DEFAULT_DATA_FILE);

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

    [JsonIgnore]
    public ObservableCollection<CommandTimerViewModel> RelevantCommandTimers { get; } = [];
    [JsonIgnore]
    private string _UserSearchEntry = string.Empty;
    [JsonIgnore]
    public string UserSearchEntry { get => _UserSearchEntry; set => SetProperty(ref _UserSearchEntry, value); }


    [JsonIgnore]
    public ObservableCollection<MenuItemViewModel_CommandTimers> QuickFilters { get; }
    [JsonIgnore]
    public ObservableCollection<MenuItemViewModel_CommandTimers> SortStrategies { get; }
    [JsonIgnore]
    public ObservableCollection<MenuItemViewModel_CommandTimers> SortDirections { get; }
    [JsonIgnore]
    public ObservableCollection<MenuItemViewModel_CommandTimers> SortOptions { get; } = [];
    [JsonIgnore]
    public ObservableCollection<MenuItemViewModel> ThemeSelections { get; } = [];
    [JsonIgnore]
    public ObservableCollection<MenuItemViewModel> LibrarySelections { get; } = [];

    //...

    [JsonIgnore]
    private CommandTimerLibrary _ActiveLibrary = new() { LibraryName = LibraryManager.DEFAULT_LIBRARY };
    [JsonIgnore]
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

    [JsonIgnore]
    private string _LibrarySelection = string.Empty;
    [JsonInclude]
    [JsonPropertyName("LibrarySelection")]
    public string Serialized_LibrarySelection {
        get => _LibrarySelection;
        set {
            if (SetProperty(ref _LibrarySelection, value, Save.No, Notify.Yes, nameof(LibrarySelection))) {
                LibraryManager.LoadLibraryToCurrent(value);
            }
        }
    }

    [JsonIgnore]
    public MenuItemViewModel LibrarySelection {
        get {
            var menuItem = _libraryMenu.GetMenuItem(_LibrarySelection) ?? LibrarySelections[0];
            Serialized_LibrarySelection = menuItem.Header;
            return menuItem;
        }

        set {
            if (value is null) return;
            if (SetProperty(ref _LibrarySelection, value.Header, Save.Yes, Notify.Yes)) {
                Serialized_LibrarySelection = value.Header;
                LibraryManager.LoadLibraryToCurrent(value.Header);
            }
        }
    }

    //...

    [JsonIgnore]
    private string _QuickFilter;
    [JsonInclude]
    [JsonPropertyName("QuickFilter")]
    private string Serialized_QuickFilter {
        get => _QuickFilter;
        set {
            if (SetProperty(ref _QuickFilter, value, Save.No, Notify.Yes, nameof(QuickFilter))) {
                SortRelevantTimers();
            }
        }
    }

    [JsonIgnore]
    public MenuItemViewModel_CommandTimers QuickFilter {
        get {
            if (GetMenuItem(QuickFilters, _QuickFilter) is not MenuItemViewModel_CommandTimers menuItem) {
                menuItem = QuickFilters[0];
                Serialized_QuickFilter = menuItem.Header;
            }

            return menuItem;
        }

        private set {
            if (SetProperty(ref _QuickFilter, value.Header, Save.Yes, Notify.Yes)) {
                SortRelevantTimers();
            }
        }
    }

    //...

    [JsonIgnore]
    private string _SortStrategy;
    [JsonInclude]
    [JsonPropertyName("SortStrategy")]
    private string Serialized_SortStrategy {
        get => _SortStrategy;
        set {
            if (SetProperty(ref _SortStrategy, value, Save.No, Notify.Yes, nameof(SortStrategy))) {
                SortRelevantTimers();
            }
        }
    }

    [JsonIgnore]
    public MenuItemViewModel_CommandTimers SortStrategy {
        get {
            if (GetMenuItem(SortStrategies, _SortStrategy) is not MenuItemViewModel_CommandTimers menuItem) {
                menuItem = SortStrategies[0];
                Serialized_SortStrategy = menuItem.Header;
            }

            return menuItem;
        }

        private set {
            if (SetProperty(ref _SortStrategy, value.Header, Save.Yes, Notify.Yes)) {
                SortRelevantTimers();
            }
        }
    }

    //...

    [JsonIgnore]
    private string _SortDirection;
    [JsonInclude]
    [JsonPropertyName("SortDirection")]
    private string Serialized_SortDirection {
        get => _SortDirection;
        set {
            if (SetProperty(ref _SortDirection, value, Save.No, Notify.Yes, nameof(SortDirection))) {
                SortRelevantTimers();
            }
        }
    }

    [JsonIgnore]
    public MenuItemViewModel_CommandTimers SortDirection {
        get {
            if (GetMenuItem(SortDirections, _SortDirection) is not MenuItemViewModel_CommandTimers menuItem) {
                menuItem = SortDirections[0];
                Serialized_SortDirection = menuItem.Header;
            }

            return menuItem;
        }

        private set {
            if (SetProperty(ref _SortDirection, value.Header, Save.Yes, Notify.Yes)) {
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

            var delay = Core.Settings.ShouldAnimate.Value is Core.Settings.AnimationChoice.All ? 100 : 50;
            for (; e < list.Count; e++) {
                if (i++ < FULL_ANIMATION_BEFORE_RAMP) {
                    await Task.Delay(delay, _cts.Token);
                    if (e > existingCount - 1) {
                        RelevantCommandTimers.Add(list[e]);
                    }
                    else {
                        RelevantCommandTimers[e] = list[e];
                    }
                    if (Core.Settings.ShouldStripeList.Value) {
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
                        if (Core.Settings.ShouldStripeList.Value) {
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
            LibraryManager.LoadLibraryToCurrent(LibrarySelection.Header);
        }
    }
}
