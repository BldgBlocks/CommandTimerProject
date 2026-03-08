using Avalonia.Media;
using System;

namespace CommandTimer.Core.Utilities;

/// <summary>
/// Platform-agnostic color utilities.
/// Pure functions with no framework dependencies beyond Avalonia.Media.Color struct.
/// </summary>
public static class ColorUtilities {

    /// <summary>
    /// Converts a Color to hex string in #AARRGGBB format.
    /// </summary>
    public static string ToHex(Color c) => $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";

    /// <summary>
    /// Parses a hex string to a Color object. Assumes the format is #AARRGGBB.
    /// If the alpha is missing, it assumes full opacity (FF).
    /// Supports both #RRGGBB and #AARRGGBB formats. Leading '0x' and '#' are stripped.
    /// </summary>
    /// <param name="hexCode">The hex color code string.</param>
    /// <returns>A Color object representing the hex code.</returns>
    /// <exception cref="FormatException">Thrown if hex string is less than 6 characters.</exception>
    public static Color ParseHexToColor(string hexCode) {
        hexCode = hexCode.Replace("0x", "").Replace("#", "");

        if (hexCode.Length < 6) {
            throw new FormatException("Hex color string is too short. Expected at least #RRGGBB.");
        }

        byte a = 255;
        byte r, g, b;

        var numberStyle = System.Globalization.NumberStyles.HexNumber;
        if (hexCode.Length >= 8) {
            a = byte.Parse(hexCode.Substring(0, 2), numberStyle);
            r = byte.Parse(hexCode.Substring(2, 2), numberStyle);
            g = byte.Parse(hexCode.Substring(4, 2), numberStyle);
            b = byte.Parse(hexCode.Substring(6, 2), numberStyle);
        }
        else {
            r = byte.Parse(hexCode.Substring(0, 2), numberStyle);
            g = byte.Parse(hexCode.Substring(2, 2), numberStyle);
            b = byte.Parse(hexCode.Substring(4, 2), numberStyle);
        }

        return new Color(a, r, g, b);
    }

    /// <summary>
    /// Calculates a contrasting foreground color (black or white) based on background luminance.
    /// Uses standard luminance formula: 0.299*R + 0.587*G + 0.114*B
    /// </summary>
    /// <param name="backgroundColor">The background color to calculate contrast against.</param>
    /// <returns>Black for light backgrounds, white for dark backgrounds.</returns>
    public static Color GetSlidingContrastColor(Color backgroundColor) {
        double luminance = 0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B;
        double contrastValue = luminance / 255;

        int r = (int)((1 - contrastValue) * 255);
        int g = (int)((1 - contrastValue) * 255);
        int b = (int)((1 - contrastValue) * 255);

        return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
    }
}


