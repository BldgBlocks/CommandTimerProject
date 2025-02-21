using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace CommandTimer.Desktop.Views;

public partial class InteractionBlocker : UserControl {

    private Panel? _layoutPanel;
    public bool Blocking { get; private set; } = false;

    //...
    public InteractionBlocker() {
        InitializeComponent();
        Focusable = true;
        IsTabStop = true;
    }

    //...
    private static void OnKeyDown(object? sender, KeyEventArgs args) {
        /// Prevent default tab behavior
        if (args.Key == Key.Tab) {
            args.Handled = true;
            return;
        }
        //if (args.Key is Key.Enter) {
        //    args.Handled = false;
        //    return;
        //}
        //if (args.Key is Key.Escape) {
        //    args.Handled = false;
        //    return;
        //}
        //args.Handled = true;
    }

    //...
    public static InteractionBlocker Create() => new();

    public InteractionBlocker Show(Panel layoutPanel) {
        _layoutPanel = layoutPanel;

        _layoutPanel.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Bubble);

        _layoutPanel.Children.Add(this);

        Blocking = true;
        PART_BorderBlocker.IsEnabled = true;
        PART_BorderBlocker.IsVisible = true;
        PART_BorderBlocker.IsHitTestVisible = true;
        Focus();
        return this;
    }

    public InteractionBlocker Hide() {
        _layoutPanel?.Children.Remove(this);

        _layoutPanel?.RemoveHandler(KeyDownEvent, OnKeyDown);

        Blocking = false;
        PART_BorderBlocker.IsEnabled = false;
        PART_BorderBlocker.IsVisible = false;
        PART_BorderBlocker.IsHitTestVisible = false;
        return this;
    }

    public InteractionBlocker WithBackground(IBrush brush) {
        PART_BorderBlocker.Background = brush;
        return this;
    }

    public InteractionBlocker WithBlurRadius(int strength) {
        PART_BorderBlocker.Effect = new BlurEffect() {
            Radius = strength 
        };
        return this;
    }
}