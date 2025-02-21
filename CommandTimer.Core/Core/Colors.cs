using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandTimer.Core;

public static class Colors {

    private static readonly Dictionary<string, Color> DarkDefaults = [];
    private static readonly Dictionary<string, Color> LightDefaults = [];

    private static void InitializeDefaults() {
        if (Application.Current is not Application current)
            throw new Exceptions.ApplicationException($"Application not found.");

        var properties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(p => typeof(IBrush).IsAssignableFrom(p.PropertyType))
            .ToList();

        properties.ForEach(p => {
            if (TryGetBrush(p.Name, out var darkBrush, ThemeVariant.Dark)) {
                DarkDefaults[p.Name] = darkBrush.Color;
            }
            if (TryGetBrush(p.Name, out var lightBrush, ThemeVariant.Light)) {
                LightDefaults[p.Name] = lightBrush.Color;
            }
        });
    }

    /// <summary>
    /// Supports Avalonia.Media.Color #AARRGGBB color definition.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static string ToHex(Color c) => $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";

    /// <summary>
    /// Parses a hex string to a Color object. Assumes the format is always #AARRGGBB.
    /// If the alpha is missing, it assumes full opacity (FF).
    /// Supports Avalonia.Media.Color uses #AARRGGBB color definition.
    /// </summary>
    /// <param name="hexCode">The hex color code string.</param>
    /// <returns>A Color object representing the hex code.</returns>
    public static Color ParseHexToColor(string hexCode) {
        hexCode = hexCode.Replace("0x", "").Replace("#", "");

        if (hexCode.Length < 6) {
            throw new FormatException("Hex color string is too short. Expected at least #RRGGBB.");
        }

        byte a = 255;
        byte r, g, b;

        var numberStyle = System.Globalization.NumberStyles.HexNumber;
        /// Avalonia.Media.Color uses #AARRGGBB color definition.
        if (hexCode.Length >= 8) {
            a = byte.Parse(hexCode.Substring(0, 2), numberStyle);
            r = byte.Parse(hexCode.Substring(2, 2), numberStyle);
            g = byte.Parse(hexCode.Substring(4, 2), numberStyle);
            b = byte.Parse(hexCode.Substring(6, 2), numberStyle);
        }
        else {
            // If the hex code is exactly 6 characters long, it's in RRGGBB format
            r = byte.Parse(hexCode.Substring(0, 2), numberStyle);
            g = byte.Parse(hexCode.Substring(2, 2), numberStyle);
            b = byte.Parse(hexCode.Substring(4, 2), numberStyle);
        }

        return new Color(a, r, g, b); // Note: Avalonia's Color uses (A, R, G, B) order
    }

    public static bool TryGetDefaultBrush(string resourceKey, out SolidColorBrush brush, ThemeVariant? themeVariant = null) {
        if (Application.Current is not Application current)
            throw new Exceptions.ApplicationException($"Application not found.");

        themeVariant ??= current.RequestedThemeVariant;
        if (themeVariant == ThemeVariant.Dark) {
            if (DarkDefaults.TryGetValue(resourceKey, out var darkColor)) {
                brush = new(darkColor);
                return true;
            }
        }
        if (themeVariant == ThemeVariant.Light) {
            if (DarkDefaults.TryGetValue(resourceKey, out var lightColor)) {
                brush = new(lightColor);
                return true;
            }
        }
        brush = new SolidColorBrush(Avalonia.Media.Colors.Purple);
        return false;
    }

    public static bool TryGetBrush(string resourceKey, out SolidColorBrush brush, ThemeVariant? themeVariant = null) {
        if (Application.Current is not Application current)
            throw new Exceptions.ApplicationException($"Application not found.");
        var found = current.Resources.TryGetResource(resourceKey, themeVariant ?? current.RequestedThemeVariant, out var value);
        brush = value as SolidColorBrush ?? new SolidColorBrush(Avalonia.Media.Colors.Purple);
        return found;
    }

    public static T? GetSetting<T>(string resourceKey, ThemeVariant? themeVariant = null) {
        if (Application.Current is not Application current)
            throw new Exceptions.ApplicationException($"Application not found.");

        if (current.Resources.TryGetResource(resourceKey, themeVariant ?? current.ActualThemeVariant, out var value) is false)
            throw new KeyNotFoundException($"Settings with key {resourceKey} not found in Resources.");

        if (value is not T cast)
            throw new KeyNotFoundException($"Setting with key {resourceKey} was found but not of type '{typeof(T)}'.");
        return cast;
    }

    public static SolidColorBrush GetBrush(string resourceKey, ThemeVariant? themeVariant = null) {
        if (Application.Current is not Application current)
            throw new Exceptions.ApplicationException($"Application not found.");

        if (current.Resources.TryGetResource(resourceKey, themeVariant ?? current.ActualThemeVariant, out var value) is false)
            throw new KeyNotFoundException($"Color with key {resourceKey} not found in Resources.");

        if (value is not SolidColorBrush brush)
            throw new KeyNotFoundException($"Color with key {resourceKey} was found but not of type 'SolidColorBrush'.");

        return brush;
    }

    public static void SetBrush(string resourceKey, SolidColorBrush brush) {
        if (Application.Current is not Application current)
            throw new Exceptions.ApplicationException($"Application not found.");

        if (DarkDefaults.Count == 0) {
            InitializeDefaults();
        }

        if (current.Resources.TryGetResource(resourceKey, current.RequestedThemeVariant, out var value) is false)
            throw new KeyNotFoundException($"Color with key {resourceKey} not found in Resources.");

        if (value is not SolidColorBrush _)
            throw new KeyNotFoundException($"Color with key {resourceKey} was found but not of type 'SolidColorBrush'.");

        current.Resources[resourceKey] = brush;
    }

    public static Color GetSlidingContrastColor(Color backgroundColor) {
        // Calculate luminance of the background color
        double luminance = 0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B;

        // For white background (luminance close to 1), text should be black (0)
        // For black background (luminance close to 0), text should be white (1)
        double contrastValue = luminance / 255;

        // Now, interpolate between black and white based on contrast value
        // Here, we invert the contrastValue to get the correct text color
        int r = (int)((1 - contrastValue) * 255);
        int g = (int)((1 - contrastValue) * 255);
        int b = (int)((1 - contrastValue) * 255);

        return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
    }


    public static Color ApplicationColor_Background => ApplicationBrush_Background.Color;
    public static SolidColorBrush ApplicationBrush_Background { get => GetBrush(nameof(ApplicationBrush_Background)); set => SetBrush(nameof(ApplicationBrush_Background), value); }

    public static Color ApplicationColor_Contrast => ApplicationBrush_Contrast.Color;
    public static SolidColorBrush ApplicationBrush_Contrast { get => GetBrush(nameof(ApplicationBrush_Contrast)); set => SetBrush(nameof(ApplicationBrush_Contrast), value); }

    public static Color ApplicationColor_Overlay => ApplicationBrush_Overlay.Color;
    public static SolidColorBrush ApplicationBrush_Overlay { get => GetBrush(nameof(ApplicationBrush_Overlay)); set => SetBrush(nameof(ApplicationBrush_Overlay), value); }

    public static Color ApplicationColor_Accent => ApplicationBrush_Accent.Color;
    public static SolidColorBrush ApplicationBrush_Accent {
        get => GetBrush(nameof(ApplicationBrush_Accent));
        set => SetBrush(nameof(ApplicationBrush_Accent), value);
    }

    public static Color ApplicationColor_Inconspicuous => ApplicationBrush_Inconspicuous.Color;
    public static SolidColorBrush ApplicationBrush_Inconspicuous { get => GetBrush(nameof(ApplicationBrush_Inconspicuous)); set => SetBrush(nameof(ApplicationBrush_Inconspicuous), value); }

    public static Color ApplicationColor_Stripe => ApplicationBrush_Stripe.Color;
    public static SolidColorBrush ApplicationBrush_Stripe { get => GetBrush(nameof(ApplicationBrush_Stripe)); set => SetBrush(nameof(ApplicationBrush_Stripe), value); }

    public static Color ApplicationColor_Transparent { get; } = new Color(0, 0, 0, 0);
    public static SolidColorBrush ApplicationBrush_Transparent { get; } = new SolidColorBrush(ApplicationColor_Transparent);

    public static Color ApplicationColor_DoThingIntended => ApplicationBrush_DoThingIntended.Color;
    public static SolidColorBrush ApplicationBrush_DoThingIntended { get => GetBrush(nameof(ApplicationBrush_DoThingIntended)); set => SetBrush(nameof(ApplicationBrush_DoThingIntended), value); }

    public static Color ApplicationColor_Bad => ApplicationBrush_Bad.Color;
    public static SolidColorBrush ApplicationBrush_Bad { get => GetBrush(nameof(ApplicationBrush_Bad)); set => SetBrush(nameof(ApplicationBrush_Bad), value); }

    public static Color ApplicationColor_Text => ApplicationBrush_Text.Color;
    public static SolidColorBrush ApplicationBrush_Text { get => GetBrush(nameof(ApplicationBrush_Text)); set => SetBrush(nameof(ApplicationBrush_Text), value); }

    public static Color ApplicationColor_Highlight => ApplicationBrush_Highlight.Color;
    public static SolidColorBrush ApplicationBrush_Highlight { get => GetBrush(nameof(ApplicationBrush_Highlight)); set => SetBrush(nameof(ApplicationBrush_Highlight), value); }
}
