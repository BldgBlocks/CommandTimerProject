using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace CommandTimer.Core.ViewModels.MenuItems;


public class MenuItemViewModel : ViewModelBase {

    public static readonly List<MenuItemViewModel> Empty = [];

    public MenuItemViewModel(string header, IEnumerable<MenuItemViewModel> items, RelayCommand<object?> menuItemSelected, RelayCommand<object?>? secondaryCommand = null) {
        _Header = header;
        _Items = items;
        _Command = menuItemSelected;
        _Parameter = this;
        _BackgroundColor = new SolidColorBrush(Avalonia.Media.Colors.Transparent);

        Command_2 = secondaryCommand;
        Parameter_2 = this;
    }


    private IEnumerable<MenuItemViewModel> _Items;

    /// <summary>
    /// 
    /// </summary>
    private string _Header;
    public string Header { get => _Header; private set => SetProperty(ref _Header, value); }

    /// <summary>
    /// 
    /// </summary>

    public IEnumerable<MenuItemViewModel> Items { get => _Items; private set => SetProperty(ref _Items, value); }

    /// <summary>
    /// 
    /// </summary>
    private RelayCommand<object?> _Command;
    public RelayCommand<object?> Command { get => _Command; init => SetProperty(ref _Command, value); }

    /// <summary>
    /// 
    /// </summary>
    private RelayCommand<object?>? _Command_2;
    public RelayCommand<object?>? Command_2 { get => _Command_2; init => SetProperty(ref _Command_2, value); }

    /// <summary>
    /// 
    /// </summary>
    private object? _Parameter;
    public object? Parameter { get => _Parameter; private set => SetProperty(ref _Parameter, value); }

    /// <summary>
    /// 
    /// </summary>
    private object? _Parameter_2;
    public object? Parameter_2 { get => _Parameter_2; private set => SetProperty(ref _Parameter_2, value); }

    /// <summary>
    /// 
    /// </summary>
    private bool _IsSelected;
    public bool IsSelected { get => _IsSelected; set => SetProperty(ref _IsSelected, value); }

    /// <summary>
    /// 
    /// </summary>
    private SolidColorBrush _BackgroundColor;
    public SolidColorBrush BackgroundColor { get => _BackgroundColor; set => SetProperty(ref _BackgroundColor, value); }
}
