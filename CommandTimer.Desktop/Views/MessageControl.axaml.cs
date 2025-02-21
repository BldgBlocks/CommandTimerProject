using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommandTimer.Core.ViewModels;
using System;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Views;

public partial class MessageControl : UserControl {

    //...
    private readonly DoubleTransition? _fadeInTransition = new() {
        Duration = TimeSpan.FromMilliseconds(100),
        Easing = new LinearEasing(),
        Property = OpacityProperty,
    };
    private readonly ThicknessTransition? _slideInTransition = new() {
        Duration = TimeSpan.FromMilliseconds(600),
        Easing = new CircularEaseInOut(),
        Property = PaddingProperty,
    };

    //...

    public MessageControl() {
        InitializeComponent();

        /// Animation Preset
        Opacity = 0;
        Padding = new Thickness(400, 0, 0, 0);
    }

    //...

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        if (DataContext is not MessageControlViewModel viewModel) return;

        viewModel.Subscribe(PlayRemoveAnimation);

        BindAnimations();

        Application.Current!.ActualThemeVariantChanged += EventHandler_ActualThemeVariantChanged;
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        if (DataContext is not MessageControlViewModel viewModel) return;

        viewModel.Unsubscribe(PlayRemoveAnimation);

        Application.Current!.ActualThemeVariantChanged -= EventHandler_ActualThemeVariantChanged;
    }

    //...

    public async void BindAnimations() {
        /// Animation Preset
        Opacity = 0;
        Padding = new Thickness(400, 0, 0, 0);

        Transitions = [
            _slideInTransition,
            _fadeInTransition,
        ];

        try {
            /// Start the transition on the next frame so previous values are set to avoid flicker. Startup only.
            await Dispatcher.UIThread.InvokeAsync(async () => {
                if (DataContext is MessageControlViewModel vm) {
                    await Task.Yield();

                    Opacity = 1;
                    Padding = new Thickness(0, 0, 0, 0);
                }
            }, DispatcherPriority.Background);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(BindAnimations)} has thrown the following.", ex);
        }
    }

    private void EventHandler_ActualThemeVariantChanged(object? sender, EventArgs e) {
        if (DataContext is not MessageControlViewModel viewModel) return;

        viewModel.Background = Core.Colors.ApplicationBrush_Overlay;
        viewModel.Foreground = Core.Colors.ApplicationBrush_Text;
    }

    private async Task PlayRemoveAnimation() {
        if (DataContext is not MessageControlViewModel viewModel) return;
        Padding = new Thickness(400, 0, 0, 0);
        try {
            await Task.Delay(_slideInTransition?.Duration.Milliseconds ?? 500);
        }
        catch (ArgumentOutOfRangeException ex) {
            Core.MessageRelay.OnMessagePosted($"{nameof(MessageControl)}>{nameof(PlayRemoveAnimation)}", $"{ex.Message}");
            throw;
        }
    }
}