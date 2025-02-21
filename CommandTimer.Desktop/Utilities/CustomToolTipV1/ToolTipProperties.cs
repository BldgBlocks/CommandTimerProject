using Avalonia.Controls.Primitives;
using Avalonia.Media;
using CommandTimer.Core;

namespace CommandTimer.Desktop.Utilities;

public record ToolTipProperties : IShowToolTipProperties {
    public IBrush? Background { get; set; }
    public Popup Reference { get; set; } = new Popup();
}
