using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CommandTimer.Core.Utilities;

namespace CommandTimer.Desktop.Utilities;

public static class ToolTipDefaults {

    /// <returns>A new instance of ToolTipProperties.</returns>
    public static ToolTipProperties GetDefaults() => new() {
        Background = Core.Colors.ApplicationBrush_Overlay,
        Reference = new Popup() {
            Placement = PlacementMode.Top,
            PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Bottom,
            VerticalOffset = -5,
            HorizontalOffset = 0
        }
    };
}
