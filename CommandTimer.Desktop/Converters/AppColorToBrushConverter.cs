using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace CommandTimer.Desktop.Converters;

/// <summary>
/// Converts AppColor to SolidColorBrush for XAML bindings.
/// Uses the cached brush pool via AppColorExtensions.ToBrush().
/// </summary>
public class AppColorToBrushConverter : IValueConverter {

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is AppColor color) {
            return color.AsBrush();
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is SolidColorBrush brush) {
            return new AppColor(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
        }
        return null;
    }
}
