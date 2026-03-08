using CommandTimer.Core.Utilities.ExtensionMethods;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CommandTimer.Core.ViewModels.MenuItems;

public record LibrarySelectionsMenu : Menu<MenuItemViewModel> {

    private readonly RelayCommand<object?> _selected;
    private readonly RelayCommand<object?> _removed;
    private readonly List<MenuItemViewModel> _items = [];

    public LibrarySelectionsMenu(Action<object?> menuItemSelected, Action<object?> menuItemRemoved) {
        _selected = new RelayCommand<object?>(menuItemSelected);
        _removed = new RelayCommand<object?>(menuItemRemoved);
        Items = _items;
        ReloadItems();
    }

    public override string Name { get; } = "Library Selections";
    public override string Description { get; } = "Select a library to load.";
    public override IEnumerable<MenuItemViewModel> Items { get; }
    /// <summary>
    /// Clear and reload collection with new items.
    /// </summary>
    public void ReloadItems(in Collection<MenuItemViewModel> collection) {
        ReloadItems();
        collection.Clear();
        foreach (var item in Items) {
            collection.Add(item);
        }
    }
    public void ReloadItems() {
        _items.Clear();
        ServiceProvider.Get<ILibraryManager>().LibraryNames.ForEach(library => {
            _items.Add(new MenuItemViewModel(library, MenuItemViewModel.Empty, _selected, _removed));
        });
    }
    public MenuItemViewModel? GetMenuItem(string itemName)
        => Items.FirstOrDefault((item) => item.Header == itemName);
}

