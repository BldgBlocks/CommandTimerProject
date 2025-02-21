using CommandTimer.Core.Utilities;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace CommandTimer.Core.ViewModels.MenuItems;

public record SortingStrategiesMenu : Menu<MenuItemViewModel_CommandTimers> {
    /// <summary>
    /// Sorting actions as opposed to filtering.
    /// </summary>
    /// <param name="menuItemSelected">The object is always the <cref cref="MenuItemViewModel"/> instance that was selected.</param>
    public SortingStrategiesMenu(Action<object?> menuItemSelected) {
        var selected = new RelayCommand<object?>(menuItemSelected);
        Items = [
            new MenuItemViewModel_CommandTimers("Name", MenuItemViewModel.Empty, selected, (l) => l.SortByName()),
            new MenuItemViewModel_CommandTimers("Time", MenuItemViewModel.Empty, selected, (l) => l.SortByTime()),
        ];
    }

    public override string Name { get; } = "Sorting Strategies";
    public override string Description { get; } = "Sorting as opposed to filtering.";
    public override IEnumerable<MenuItemViewModel_CommandTimers> Items { get; }
}
