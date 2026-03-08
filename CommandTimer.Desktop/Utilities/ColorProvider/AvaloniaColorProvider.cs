using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommandTimer.Core.Static.Exceptions;
using CommandTimer.Core.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace CommandTimer.Desktop.Utilities.ColorProvider;

/// <summary>
/// Avalonia-specific implementation of IColorProvider.
/// Each color is an ObservableProperty. Consumers subscribe to ValueChanged events.
/// Purple fallback brush signals missing resources (intentional debug aid).
/// </summary>
public class AvaloniaColorProvider : IColorProvider {

    private readonly Dictionary<string, Color> _darkDefaults = [];
    private readonly Dictionary<string, Color> _lightDefaults = [];

    public AvaloniaColorProvider() {
        InitializeDefaults();
        InitializeObservables();
    }

    private void InitializeDefaults() {
        if (Application.Current is not Application current)
            throw new Exceptions.ApplicationException($"Application not found.");

        var properties = typeof(IColorProvider).GetProperties()
            .Where(p => p.PropertyType == typeof(ResourceBackedObservableProperty))
            .ToList();

        properties.ForEach(p => {
            if (TryGetBrushFromResources(p.Name, out var darkBrush, ThemeVariant.Dark)) {
                _darkDefaults[p.Name] = darkBrush.Color;
            }
            if (TryGetBrushFromResources(p.Name, out var lightBrush, ThemeVariant.Light)) {
                _lightDefaults[p.Name] = lightBrush.Color;
            }
        });
    }

    private void InitializeObservables() {
        ApplicationBrush_Background = new(nameof(ApplicationBrush_Background));
        ApplicationBrush_Contrast = new(nameof(ApplicationBrush_Contrast));
        ApplicationBrush_Overlay = new(nameof(ApplicationBrush_Overlay));
        ApplicationBrush_Accent = new(nameof(ApplicationBrush_Accent));
        ApplicationBrush_Inconspicuous = new(nameof(ApplicationBrush_Inconspicuous));
        ApplicationBrush_Stripe = new(nameof(ApplicationBrush_Stripe));
        ApplicationBrush_Transparent = new(new SolidColorBrush(new Color(0, 0, 0, 0)));
        ApplicationBrush_DoThingIntended = new(nameof(ApplicationBrush_DoThingIntended));
        ApplicationBrush_Bad = new(nameof(ApplicationBrush_Bad));
        ApplicationBrush_Text = new(nameof(ApplicationBrush_Text));
        ApplicationBrush_Highlight = new(nameof(ApplicationBrush_Highlight));
    }

    public bool TryGetDefaultBrush(string resourceKey, out SolidColorBrush brush, ThemeVariant? themeVariant = null) {
        if (Application.Current is not Application current)
            throw new Exceptions.ApplicationException($"Application not found.");

        themeVariant ??= current.RequestedThemeVariant;
        if (themeVariant == ThemeVariant.Dark) {
            if (_darkDefaults.TryGetValue(resourceKey, out var darkColor)) {
                brush = new(darkColor);
                return true;
            }
        }
        if (themeVariant == ThemeVariant.Light) {
            if (_lightDefaults.TryGetValue(resourceKey, out var lightColor)) {
                brush = new(lightColor);
                return true;
            }
        }
        brush = new SolidColorBrush(Avalonia.Media.Colors.Purple);
        return false;
    }

    private static bool TryGetBrushFromResources(string resourceKey, out SolidColorBrush brush, ThemeVariant? themeVariant = null) {
        if (Application.Current is not Application current)
            throw new Exceptions.ApplicationException($"Application not found.");
        var found = current.Resources.TryGetResource(resourceKey, themeVariant ?? current.ActualThemeVariant, out var value);
        brush = value as SolidColorBrush ?? new SolidColorBrush(Avalonia.Media.Colors.Purple);
        return found;
    }

    private static SolidColorBrush GetBrushFromResources(string resourceKey) {
        if (Application.Current is not Application current)
            throw new Exceptions.ApplicationException($"Application not found.");

        if (current.Resources.TryGetResource(resourceKey, current.ActualThemeVariant, out var value) is false)
            return new SolidColorBrush(Avalonia.Media.Colors.Purple);

        if (value is not SolidColorBrush brush)
            return new SolidColorBrush(Avalonia.Media.Colors.Purple);

        return brush;
    }

    //... Interface Required Properties
    public ResourceBackedObservableProperty ApplicationBrush_Background { get; private set; } = null!;
    public ResourceBackedObservableProperty ApplicationBrush_Contrast { get; private set; } = null!;
    public ResourceBackedObservableProperty ApplicationBrush_Overlay { get; private set; } = null!;
    public ResourceBackedObservableProperty ApplicationBrush_Accent { get; private set; } = null!;
    public ResourceBackedObservableProperty ApplicationBrush_Inconspicuous { get; private set; } = null!;
    public ResourceBackedObservableProperty ApplicationBrush_Stripe { get; private set; } = null!;
    public ResourceBackedObservableProperty ApplicationBrush_Transparent { get; private set; } = null!;
    public ResourceBackedObservableProperty ApplicationBrush_DoThingIntended { get; private set; } = null!;
    public ResourceBackedObservableProperty ApplicationBrush_Bad { get; private set; } = null!;
    public ResourceBackedObservableProperty ApplicationBrush_Text { get; private set; } = null!;
    public ResourceBackedObservableProperty ApplicationBrush_Highlight { get; private set; } = null!;
}


