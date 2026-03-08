using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace CommandTimer.Core.Utilities.DependencyInversion;

public interface IShowToolTipProperties {
    public IBrush? Background { get; set; }
    /// <summary>
    /// A reference object to configure the popup with when shown.
    /// </summary>
    public Popup Reference { get; set; }
}

