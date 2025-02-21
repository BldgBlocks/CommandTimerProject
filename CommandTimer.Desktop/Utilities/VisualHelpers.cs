using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using System.Globalization;

namespace CommandTimer.Desktop.Utilities;
public static class VisualHelpers {

    public static bool IsOverlapping(this Control control1, Control control2) {
        if (control1 == null || control2 == null)
            return false;

        var parent1 = control1.GetVisualParent();
        var parent2 = control2.GetVisualParent();

        if (parent1 == parent2) {
            return IsOverlappingInLocalSpace(control1, control2);
        }
        else {
            return IsOverlappingInGlobalSpace(control1, control2);
        }
    }

    public static bool IsOverlapping(Rect bounds1, Rect bounds2)
        => bounds1.Intersects(bounds2);

    public static bool IsOverlappingInLocalSpace(Control control1, Control control2)
        => IsOverlapping(control1.Bounds, control2.Bounds);

    // TODO: This is not tested.
    public static bool IsOverlappingInGlobalSpace(Control control1, Control control2) {
        if (control1.GetVisualParent() is not Visual visualParent1) return false;
        if (control2.GetVisualParent() is not Visual visualParent2) return false;

        var screenPos1 = control1.TransformToVisual(visualParent1)?.Transform(new Point(0, 0));
        var screenPos2 = control2.TransformToVisual(visualParent2)?.Transform(new Point(0, 0));

        var bounds1 = new Rect(screenPos1 ?? new Point(0, 0), control1.Bounds.Size);
        var bounds2 = new Rect(screenPos2 ?? new Point(100, 100), control2.Bounds.Size);

        return bounds1.Intersects(bounds2);
    }

    public static bool IsRightOf(this Control control, Control other) =>
        control.Bounds.Left > other.Bounds.Right;

    public static bool IsLeftOf(this Control control, Control other) =>
        control.Bounds.Right < other.Bounds.Left;

    public static int FindOptimalTextSize(int maxSize, int minSize, string text, TextBlock textBlock, ContentControl container, int padding = 10) {
        double availableWidth = container.Width - container.Padding.Left - container.Padding.Right - padding;
        double availableHeight = container.Height - container.Padding.Top - container.Padding.Bottom;

        while (maxSize > minSize) {
            int midSize = (maxSize + minSize) / 2;
            var neededSize = MeasureText(text, midSize, textBlock.FontFamily, availableWidth);

            if (neededSize.Width <= availableWidth && neededSize.Height <= availableHeight)
                minSize = midSize + 1;
            else
                maxSize = midSize;
        }

        return minSize - 1;
    }

    private static Size MeasureText(string text, int fontSize, FontFamily fontFamily, double maxWidth) {
        var formattedText = 
            new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(fontFamily),
                    fontSize,
                    Brushes.White
                ) {
                MaxTextWidth = maxWidth
            };

        return new Size(formattedText.Width, formattedText.Height);
    }
}
