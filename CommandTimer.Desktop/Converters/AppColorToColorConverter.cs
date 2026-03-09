using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace CommandTimer.Desktop.Converters;

/// <summary>
/// Converts AppColor to Avalonia.Media.Color for ColorView bindings.
/// </summary>
public class AppColorToColorConverter : IValueConverter {

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is AppColor color) {
            return new Color(color.A, color.R, color.G, color.B);
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is Color color) {
            return new AppColor(color.A, color.R, color.G, color.B);
        }
        return null;
    }
}
