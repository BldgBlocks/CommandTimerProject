using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Views;

public partial class PrivacyStatement : UserControl {

    //...
    private static PrivacyStatement? _instance;

    private readonly Canvas _canvas = new() {
        Name = "DisclaimerCanvas",
        ZIndex = 1001,
        Background = Brushes.Transparent,
        Focusable = true,
    };
    private Window? _window;
    private Panel? _layoutPanel;
    private InteractionBlocker? _interactionBlocker;

    public PrivacyStatement() {
        InitializeComponent();
        SetupButtonFocus();
        QuitButton.IsVisible = false;
    }

    private void SetupButtonFocus() {
        ProceedButton.GotFocus += (s, e) => {
            ProceedButton.BorderBrush = Brushes.Aquamarine;
            ProceedButton.BorderThickness = new Thickness(1);
        };
        QuitButton.GotFocus += (s, e) => {
            QuitButton.BorderBrush = Brushes.Red;
            QuitButton.BorderThickness = new Thickness(1);
        };
        ProceedButton.LostFocus += (s, e) => {
            ProceedButton.BorderThickness = new Thickness(0);
        };
        QuitButton.LostFocus += (s, e) => {
            QuitButton.BorderThickness = new Thickness(0);
        };
    }

    private void OnKeyDown(object? sender, KeyEventArgs args) {
        if (args.Key is Key.Tab) {
            if (ProceedButton.IsFocused) QuitButton.Focus();
            else if (QuitButton.IsFocused) ProceedButton.Focus();
            else ProceedButton.Focus();
        }
        else if (args.Key is Key.Enter) {
            if (QuitButton.IsFocused) Quit();
            else if (ProceedButton.IsFocused) Proceed();
            else Proceed();
        }
        else if (args.Key is Key.Escape) {
            Quit();
        }
        args.Handled = true;
    }

    //... Public Methods

    public static PrivacyStatement Get() => _instance ??= new();

    public PrivacyStatement Show(Panel layoutControl) {
        _layoutPanel = layoutControl;

        _layoutPanel.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Bubble);
        _interactionBlocker = InteractionBlocker.Create().Show(layoutControl);

        if (_canvas.Children.Contains(this) is false) {
            _canvas.Children.Add(this);
        }
        if (_layoutPanel.Children.Contains(_canvas) is false) {
            _layoutPanel.Children.Add(_canvas);
        }
        SetCenterOfCanvas(_canvas, this);

        try {
            Dispatcher.UIThread.Invoke(async () => {
                await Task.Yield();

                _window = TopLevel.GetTopLevel(_layoutPanel) as Window;

                _window!.Resized += Window_Resized;
                SetCenterOfCanvas(_canvas, this);

                _canvas.Focus();

                await Task.Delay(100);
                SetCenterOfCanvas(_canvas, this);

                await Task.Delay(500);
                SetCenterOfCanvas(_canvas, this);
            }, DispatcherPriority.Render);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            throw new Exception($"Method: {nameof(Show)} has thrown the following.", ex);
        }
        return this;
    }

    public PrivacyStatement Hide() {
        if (_window is not null) {
            _window.Resized -= Window_Resized;
        }
        _layoutPanel?.RemoveHandler(KeyDownEvent, OnKeyDown);
        _layoutPanel?.Children.Remove(_canvas);
        _interactionBlocker?.Hide();
        return this;
    }

    //... Event Handlers

    private void Window_Resized(object? sender, WindowResizedEventArgs e)
        => SetCenterOfCanvas(_canvas, this);

    private void Click_ProceedButton(object? sender, RoutedEventArgs args)
        => Proceed();

    private void Click_QuitButton(object? sender, RoutedEventArgs args)
        => Quit();

    //... Actions

    private void Proceed()
        => Hide();

    private static void Quit()
        => App.Shutdown();

    private static void SetCenterOfCanvas(Canvas canvas, UserControl control) {
        double canvasWidth = canvas.Bounds.Width;
        double canvasHeight = canvas.Bounds.Height;
        double controlWidth = control.Bounds.Width;
        double controlHeight = control.Bounds.Height;

        double left = (canvasWidth - controlWidth) / 2;
        double top = (canvasHeight - controlHeight) / 2;

        Canvas.SetLeft(control, left);
        Canvas.SetTop(control, top);
    }
}