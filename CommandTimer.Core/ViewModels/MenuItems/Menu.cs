using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace CommandTimer.Core.ViewModels.MenuItems;

public abstract record Menu<T> where T : MenuItemViewModel {
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract IEnumerable<T> Items { get; }
}
