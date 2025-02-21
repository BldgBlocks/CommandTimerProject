using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace CommandTimer.Core.ViewModels.MenuItems;

public record ThemeSelectionsMenu : Menu<MenuItemViewModel> {
    /// <summary>
    /// Sorting actions as opposed to filtering.
    /// </summary>
    /// <param name="menuItemSelected">The object is always the <cref cref="MenuItemViewModel"/> instance that was selected.</param>
    public ThemeSelectionsMenu(Action<object?> menuItemSelected) {
        var selected = new RelayCommand<object?>(menuItemSelected);
        Items = [
            new MenuItemViewModel($"{ThemeVariant.Dark}", MenuItemViewModel.Empty, selected),
            new MenuItemViewModel($"{ThemeVariant.Light}", MenuItemViewModel.Empty, selected),
            new MenuItemViewModel($"{ThemeVariant.Default}", MenuItemViewModel.Empty, selected),
        ];
    }

    public override string Name { get; } = "Theme Selections";
    public override string Description { get; } = "Light/Dark color schemes for the application.";
    public override IEnumerable<MenuItemViewModel> Items { get; }
}
