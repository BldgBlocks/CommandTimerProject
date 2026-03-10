using Avalonia.Media;
using CommandTimer.Desktop.Utilities.ColorProvider;

namespace CommandTimer.Desktop.Utilities;

public static class AppColorExtensions {

    /// <summary>
    /// Converts an <see cref="AppColor"/> to a <see cref="SolidColorBrush"/> using the registered color provider (should be cached)
    /// If no <see cref="AvaloniaColorProvider"/> is available, returns a fallback purple brush (intentional debug signal).
    /// </summary>
    /// <param name="color">The <see cref="AppColor"/> to convert to a brush.</param>
    /// <returns>A cached <see cref="SolidColorBrush"/> for the color, or a purple fallback brush if no provider is available.</returns>
    public static SolidColorBrush AsBrush(this AppColor color) {
        if (ServiceProvider.Get<IColorProvider>() is AvaloniaColorProvider provider) {
            return provider.GetCachedBrush(color);
        }
        return new SolidColorBrush(new Avalonia.Media.Color(AppColor.Fallback.A, AppColor.Fallback.R, AppColor.Fallback.G, AppColor.Fallback.B));
    }
}