using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Views;

public partial class ConfirmationPresenter : UserControl {

    private readonly Canvas _canvas = new() {
        Name = "PART_Canvas",
        ZIndex = 1001,
        Background = Brushes.Transparent,
        Focusable = true,
        Opacity = 0,
        Transitions = [new DoubleTransition() { 
            Duration = TimeSpan.FromSeconds(0.5),
            Easing = new SineEaseOut(),
            Property = OpacityProperty,
        }]
    };
    private Panel? _layoutPanel;
    private readonly InteractionBlocker _interactionBlocker = InteractionBlocker.Create();
    private Button? _yes;
    private Button? _no;
    private Button? _cancel;

    //... 

    /// <summary>
    /// Returns a Task that doesn't await or return immediately, and can be interacted with by other methods,
    /// such as a button setting a state to return through the task.
    /// </summary>
    private readonly TaskCompletionSource<bool?> _tcs = new();

    public Task<bool?> Show(Panel panel) {
        _layoutPanel = panel;

        ShowCentered();

        return _tcs.Task;
    }

    public void Hide() {
        _canvas.Opacity = 0;
        _layoutPanel?.Children.Remove(_canvas);
        _interactionBlocker.Hide();
    }

    public ConfirmationPresenter() {
        InitializeComponent();
    }

    private async void ShowCentered() {
        SetCenterOfCanvas(_canvas, this);
        await Dispatcher.UIThread.Invoke(async () => {
            _interactionBlocker.Show(_layoutPanel!);
            _canvas.Children.Add(this);
            _layoutPanel?.Children.Add(_canvas);
            _canvas.UpdateLayout();

            while (_tcs.Task.IsCompleted is false) {
                SetCenterOfCanvas(_canvas, this);
                await Task.Delay(10);
                _canvas.Opacity = 1;
            }
        }, DispatcherPriority.Background);
    }

    private static void SetCenterOfCanvas(Canvas canvas, UserControl control) {
        double left = (canvas.Bounds.Width - control.Bounds.Width) / 2;
        double top = (canvas.Bounds.Height - control.Bounds.Height) / 2;

        Canvas.SetLeft(control, left);
        Canvas.SetTop(control, top);
    }

    public static ConfirmationPresenter Create(UserControl view, Button yes, Button? no = null, Button? cancel = null) {
        ConfirmationPresenter dialog = new();
        dialog.PART_PresenterBody.Content = view;
        dialog._yes = yes;
        dialog._no = no;
        dialog._cancel = cancel;
        if (dialog._yes is not null) {
            dialog._yes.Tapped += dialog.Tapped_ButtonYes;
        }        
        if (dialog._no is not null) {
            dialog._no.Tapped += dialog.Tapped_ButtonNo;
        }
        if (dialog._cancel is not null) {
            dialog._cancel.Tapped += dialog.Tapped_ButtonCancel;
        }
        return dialog;
    }

    private void Tapped_ButtonYes(object? sender, TappedEventArgs e) {
        _tcs.TrySetResult(true);
        Hide();
    }

    private void Tapped_ButtonNo(object? sender, TappedEventArgs e) {
        _tcs.TrySetResult(false);
        Hide();
    }

    private void Tapped_ButtonCancel(object? sender, TappedEventArgs e) {
        _tcs.TrySetResult(null);
        Hide();
    }
}