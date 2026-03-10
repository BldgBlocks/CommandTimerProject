using System.Collections.Generic;

namespace CommandTimer.Desktop.Views.Menus;

public interface IMenuItemsCollection {
    public abstract IReadOnlyList<MenuItem> Items { get; }
}


