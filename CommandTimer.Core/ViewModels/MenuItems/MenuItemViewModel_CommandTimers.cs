using CommunityToolkit.Mvvm.Input;

namespace CommandTimer.Core.ViewModels.MenuItems;

public class MenuItemViewModel_CommandTimers(string header, List<MenuItemViewModel> items, RelayCommand<object?> menuItemSelected, Action<List<CommandTimerViewModel>> menuItemAction)
    : MenuItemViewModel(header, items, menuItemSelected) {

    public Action<List<CommandTimerViewModel>> MenuItemAction { get => menuItemAction; private set => SetProperty(ref menuItemAction, value); }

}
