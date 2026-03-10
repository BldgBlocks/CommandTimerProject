using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using CommandTimer.Core.Utilities;
using System;
using System.Collections.Generic;

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
        PART_BorderBackground.Background = ServiceProvider.Get<IColorProvider>().ApplicationBrush_Overlay.Value.AsBrush();
        PART_TextPresenter.FontSize = ResourceHelper.GetResourceOrThrow<double>("ApplicationFontSize_Text");
    }

    private static T GetResourceOrThrow<T>(string resourceKey) {
        if (Application.Current is not Application current)
            throw new InvalidOperationException($"Application.Current not initialized.");

        if (current.Resources.TryGetResource(resourceKey, current.ActualThemeVariant, out var value) is false)
            throw new KeyNotFoundException($"Resource '{resourceKey}' not found in Application.Current.Resources.");

        if (value is not T typedValue)
            throw new InvalidCastException($"Resource '{resourceKey}' found but is type '{value?.GetType().Name}', expected '{typeof(T).Name}'.");

        return typedValue;
    }
}









