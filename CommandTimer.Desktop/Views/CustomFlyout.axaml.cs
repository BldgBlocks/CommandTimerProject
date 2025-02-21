using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;

namespace CommandTimer.Desktop.Views;

public partial class CustomToolTip : Popup {
    public CustomToolTip() {
        InitializeComponent();
        WindowManagerAddShadowHint = true;
        IsLightDismissEnabled = true;
        VerticalOffset = -5;
        Placement = PlacementMode.Top;
        PlacementAnchor = PopupAnchor.Bottom;

        /// Popups can not access Resources.
        PART_BorderBackground.Background = Core.Colors.ApplicationBrush_Overlay;
        PART_TextPresenter.FontSize = Core.Colors.GetSetting<double>("ApplicationFontSize_Text");
    }
}