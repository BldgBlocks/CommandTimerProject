using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace CommandTimer.Desktop.Utilities;

public static class ToolTipDefaults {

    /// <returns>A new instance of ToolTipProperties.</returns>
    public static ToolTipProperties GetDefaults() => new() {
        Background = ServiceProvider.Get<IColorProvider>().ApplicationBrush_Overlay.Value,
        Reference = new Popup() {
            Placement = PlacementMode.Top,
            PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Bottom,
            VerticalOffset = -5,
            HorizontalOffset = 0
        }
    };
}



