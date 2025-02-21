using Avalonia.Controls;
using System.Collections.Generic;

namespace CommandTimer.Desktop.Views;

public interface IMenuItemsCollection {
    public abstract IReadOnlyList<MenuItem> Items { get; }
}
