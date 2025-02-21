using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace CommandTimer.Core.ViewModels.MenuItems;

public record SortingDirectionsMenu : Menu<MenuItemViewModel_CommandTimers> {
    /// <summary>
    /// Sorting actions as opposed to filtering.
    /// </summary>
    /// <param name="menuItemSelected">The object is always the <cref cref="MenuItemViewModel"/> instance that was selected.</param>
    public SortingDirectionsMenu(Action<object?> menuItemSelected) {
        var selected = new RelayCommand<object?>(menuItemSelected);
        Items = [
            new MenuItemViewModel_CommandTimers("Ascending", MenuItemViewModel.Empty, selected, (l) => { }),
            new MenuItemViewModel_CommandTimers("Descending", MenuItemViewModel.Empty, selected, (l) => l.Reverse()),
        ];
    }

    public override string Name { get; } = "Sorting Direction";
    public override string Description { get; } = "Change direction of final sort order.";
    public override IEnumerable<MenuItemViewModel_CommandTimers> Items { get; }
}
