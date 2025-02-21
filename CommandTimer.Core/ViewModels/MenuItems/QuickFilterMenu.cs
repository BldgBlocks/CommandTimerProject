using CommandTimer.Core.Utilities;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace CommandTimer.Core.ViewModels.MenuItems;

public record QuickFilterMenu : Menu<MenuItemViewModel_CommandTimers> {
    /// <summary>
    /// Sorting actions as opposed to filtering.
    /// </summary>
    /// <param name="menuItemSelected">The object is always the <cref cref="MenuItemViewModel"/> instance that was selected.</param>
    public QuickFilterMenu(Action<object?> menuItemSelected) {
        var selected = new RelayCommand<object?>(menuItemSelected);
        Items = [
            new MenuItemViewModel_CommandTimers("All", MenuItemViewModel.Empty, selected, (l) => l.FilterByCondition((t) => true) ),
            new MenuItemViewModel_CommandTimers("Inactive", MenuItemViewModel.Empty, selected, (l) => l.FilterByCondition((t) => !t.IsActive) ),
            new MenuItemViewModel_CommandTimers("Active", MenuItemViewModel.Empty, selected, (l) => l.FilterByCondition((t) => t.IsActive) ),

            new MenuItemViewModel_CommandTimers("Less Than 10 Minutes", MenuItemViewModel.Empty, selected, (l) => { l.FilterByCondition((t) => t.TimeSpanTillExecution <= TimeSpan.FromMinutes(10)); }),
            new MenuItemViewModel_CommandTimers("Less Than 30 Minutes", MenuItemViewModel.Empty, selected, (l) => { l.FilterByCondition((t) => t.TimeSpanTillExecution <= TimeSpan.FromMinutes(30)); }),
            new MenuItemViewModel_CommandTimers("Less Than 60 Minutes", MenuItemViewModel.Empty, selected, (l) => { l.FilterByCondition((t) => t.TimeSpanTillExecution <= TimeSpan.FromMinutes(60)); }),
            new MenuItemViewModel_CommandTimers("More Than 60 Minutes", MenuItemViewModel.Empty, selected, (l) => { l.FilterByCondition((t) => t.TimeSpanTillExecution >= TimeSpan.FromMinutes(60)); }),
            new MenuItemViewModel_CommandTimers("More Than 3 Hours", MenuItemViewModel.Empty, selected, (l) => { l.FilterByCondition((t) => t.TimeSpanTillExecution >= TimeSpan.FromMinutes(180)); }),

            new MenuItemViewModel_CommandTimers("Auto Start", MenuItemViewModel.Empty, selected, (l) => l.FilterByCondition((t) => t.IsAutoStart) ),
            new MenuItemViewModel_CommandTimers("Duration Target", MenuItemViewModel.Empty, selected, (l) => l.FilterByCondition((t) => t.TimeMode.Equals(CommandTimerViewModel.TimeModeChoice.Duration))),
            new MenuItemViewModel_CommandTimers("Time Target", MenuItemViewModel.Empty, selected, (l) => l.FilterByCondition((t) => t.TimeMode.Equals(CommandTimerViewModel.TimeModeChoice.Time))),
            new MenuItemViewModel_CommandTimers("Date Target", MenuItemViewModel.Empty, selected, (l) => l.FilterByCondition((t) => t.TimeMode.Equals(CommandTimerViewModel.TimeModeChoice.Date))),
        ];
    }

    public override string Name { get; } = "Filtering Strategies";
    public override string Description { get; } = "Filtering as opposed to sorting.";
    public override IEnumerable<MenuItemViewModel_CommandTimers> Items { get; }
}
