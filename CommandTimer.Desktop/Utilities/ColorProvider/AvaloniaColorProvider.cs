using Avalonia.Media;
using Avalonia.Styling;
using CommandTimer.Core.Static.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace CommandTimer.Desktop.Utilities.ColorProvider;

/// <summary>
/// Avalonia-specific implementation of IColorProvider.
/// Each color is an ObservableProperty. Consumers subscribe to ValueChanged events.
/// Purple fallback brush signals missing resources (intentional debug aid).
/// </summary>
public class AvaloniaColorProvider : IColorProvider {

    private readonly Dictionary<string, Avalonia.Media.Color> _darkDefaults = [];
    private readonly Dictionary<string, Avalonia.Media.Color> _lightDefaults = [];
    private readonly Dictionary<AppColor, SolidColorBrush> _brushCache = [];

    public AvaloniaColorProvider() {
        InitializeDefaults();
        InitializeObservables();
    }

    private void InitializeDefaults() {
        if (Application.Current is not Application current)
            throw new Exceptions.ApplicationException($"Application not found.");

        var properties = typeof(IColorProvider).GetProperties()
            .Where(p => p.PropertyType == typeof(ObservableProperty<AppColor>))
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
        ApplicationBrush_Background = new ResourceBackedObservableProperty(nameof(ApplicationBrush_Background));
        ApplicationBrush_Contrast = new ResourceBackedObservableProperty(nameof(ApplicationBrush_Contrast));
        ApplicationBrush_Overlay = new ResourceBackedObservableProperty(nameof(ApplicationBrush_Overlay));
        ApplicationBrush_Accent = new ResourceBackedObservableProperty(nameof(ApplicationBrush_Accent));
        ApplicationBrush_Inconspicuous = new ResourceBackedObservableProperty(nameof(ApplicationBrush_Inconspicuous));
        ApplicationBrush_Stripe = new ResourceBackedObservableProperty(nameof(ApplicationBrush_Stripe));
        ApplicationBrush_Transparent = new ResourceBackedObservableProperty(AppColor.Transparent);
        ApplicationBrush_DoThingIntended = new ResourceBackedObservableProperty(nameof(ApplicationBrush_DoThingIntended));
        ApplicationBrush_Bad = new ResourceBackedObservableProperty(nameof(ApplicationBrush_Bad));
        ApplicationBrush_Text = new ResourceBackedObservableProperty(nameof(ApplicationBrush_Text));
        ApplicationBrush_Highlight = new ResourceBackedObservableProperty(nameof(ApplicationBrush_Highlight));
    }

    public SolidColorBrush GetCachedBrush(AppColor color) {
        if (_brushCache.TryGetValue(color, out var brush)) return brush;
        brush = new SolidColorBrush(new Avalonia.Media.Color(color.A, color.R, color.G, color.B));
        _brushCache[color] = brush;
        return brush;
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
    public ObservableProperty<AppColor> ApplicationBrush_Background { get; private set; } = null!;
    public ObservableProperty<AppColor> ApplicationBrush_Contrast { get; private set; } = null!;
    public ObservableProperty<AppColor> ApplicationBrush_Overlay { get; private set; } = null!;
    public ObservableProperty<AppColor> ApplicationBrush_Accent { get; private set; } = null!;
    public ObservableProperty<AppColor> ApplicationBrush_Inconspicuous { get; private set; } = null!;
    public ObservableProperty<AppColor> ApplicationBrush_Stripe { get; private set; } = null!;
    public ObservableProperty<AppColor> ApplicationBrush_Transparent { get; private set; } = null!;
    public ObservableProperty<AppColor> ApplicationBrush_DoThingIntended { get; private set; } = null!;
    public ObservableProperty<AppColor> ApplicationBrush_Bad { get; private set; } = null!;
    public ObservableProperty<AppColor> ApplicationBrush_Text { get; private set; } = null!;
    public ObservableProperty<AppColor> ApplicationBrush_Highlight { get; private set; } = null!;
}


