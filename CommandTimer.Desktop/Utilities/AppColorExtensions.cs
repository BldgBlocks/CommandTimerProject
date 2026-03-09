using CommandTimer.Core.Static;

namespace CommandTimer.Desktop.Utilities;

public static class AppColorExtensions {
    public static Avalonia.Media.SolidColorBrush ToBrush(this AppColor color) {
        return ServiceProvider.Get<IColorProvider>().GetCachedBrush(color);
    }
}