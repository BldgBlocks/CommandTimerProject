using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommandTimer.Core.Utilities;
using CommandTimer.Core.Utilities.DependencyInversion;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Views;

public partial class ConfirmationDialog : UserControl {

    private readonly Canvas _canvas = new() {
        Name = "PART_Canvas",
        ZIndex = 1001,
        Background = Brushes.Transparent,
        Focusable = true,
    };
    private Panel? _layoutPanel;
    private readonly InteractionBlocker _interactionBlocker = InteractionBlocker.Create();
    private Predicate<string>? _overrideValidation = null;

    //... 

    /// <summary>
    /// Returns a Task that doesn't await or return immediately, and can be interacted with by other methods,
    /// such as a button setting a state to return through the task.
    /// </summary>
    private readonly TaskCompletionSource<bool?> _tcs = new();

    public Task<bool?> Show(Panel panel) {
        _layoutPanel = panel;

        ShowCentered();

        _canvas.Focus();
        if (PART_PasswordEntry.IsVisible) {
            PART_PasswordEntry.Focus();
        }

        return _tcs.Task;
    }

    public void Hide() {
        _layoutPanel?.Children.Remove(_canvas);
        _interactionBlocker.Hide();
    }

    public ConfirmationDialog() {
        InitializeComponent();

        this.KeyDown += OnKeyDown;
        _canvas.KeyDown += OnKeyDown;
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
            }
        }, DispatcherPriority.Background);
    }

    private static void SetCenterOfCanvas(Canvas canvas, UserControl control) {
        double left = (canvas.Bounds.Width - control.Bounds.Width) / 2;
        double top = (canvas.Bounds.Height - control.Bounds.Height) / 2;

        Canvas.SetLeft(control, left);
        Canvas.SetTop(control, top);
    }

    private void Tapped_ButtonYes(object sender, TappedEventArgs e) {
        if (PART_PasswordEntry.IsVisible) {
            ValidatePassword();
        }
        else {
            _tcs.TrySetResult(true);
            Hide();
        }
    }

    private void Tapped_ButtonNo(object sender, TappedEventArgs e) {
        _tcs.TrySetResult(false);
        Hide();
    }

    private void Tapped_ButtonCancel(object sender, TappedEventArgs e) {
        _tcs.TrySetResult(null);
        Hide();
    }

    public void OnKeyDown(object? sender, KeyEventArgs args) {
        if (args.Key != Key.Enter) return;
        if (PART_PasswordEntry.IsVisible) {
            ValidatePassword();
        }
        else {
            _tcs.TrySetResult(true);
            Hide();
        }
        
        args.Handled = true;
    }

    private void ValidatePassword() {
        var passwordValidation = ServiceProvider.Get<IPasswordValidation>();
        if (passwordValidation.Validate(PART_PasswordEntry.Text ?? string.Empty) || (_overrideValidation?.Invoke(PART_PasswordEntry.Text ?? string.Empty)).GetValueOrDefault()) {
            PART_PasswordEntryTip.Foreground = Core.Colors.ApplicationBrush_DoThingIntended;
            PART_PasswordEntryTip.Text = "Granted";
            _tcs.TrySetResult(true);
            Hide();
        }
        else {
            PART_PasswordEntryTip.Foreground = Core.Colors.ApplicationBrush_Bad;
            PART_PasswordEntryTip.Text = "The password is incorrect.";
        }
    }

    //... Builder Methods

    public static ConfirmationDialog Create() => new();

    public ConfirmationDialog WithTitle(string title) {
        PART_TextTitle.Text = title;
        return this;
    }

    public ConfirmationDialog WithMessage(string message) {
        PART_TextMessage.Text = message;
        return this;
    }

    public ConfirmationDialog WithChoiceYesText(string message) {
        PART_ButtonYes.Content = message;
        return this;
    }

    public ConfirmationDialog WithChoiceNoText(string message) {
        PART_ButtonNo.Content = message;
        return this;
    }

    public ConfirmationDialog WithChoiceCancelText(string message) {
        PART_ButtonCancel.Content = message;
        return this;
    }

    public ConfirmationDialog WithYesButton(bool state = true) {
        PART_ButtonYes.IsVisible = state;
        return this;
    }

    public ConfirmationDialog WithNoButton(bool state) {
        PART_ButtonNo.IsVisible = state;
        return this;
    }

    public ConfirmationDialog WithCancelButton(bool state) {
        PART_ButtonCancel.IsVisible = state;
        return this;
    }

    public ConfirmationDialog WithMaxSize(int width, int height) {
        this.MaxWidth = Math.Max(this.MinWidth, width);
        this.MaxHeight = Math.Max(this.MinHeight, height);
        return this;
    }

    public ConfirmationDialog WithMinSize(int width, int height) {
        this.MinWidth = Math.Clamp(width, 200, this.MaxWidth);
        this.MinHeight = Math.Clamp(height, 200, this.MaxHeight);
        return this;
    }

    public ConfirmationDialog WithBorder(IBrush brush, Thickness thickness) {
        PART_BorderRoot.BorderBrush = brush;
        PART_BorderRoot.BorderThickness = thickness;
        return this;
    }

    public ConfirmationDialog WithBackground(IBrush brush) {
        PART_BorderRoot.Background = brush;
        return this;
    }

    public ConfirmationDialog WithCanvasBackground(IBrush brush) {
        _interactionBlocker.WithBackground(brush);
        return this;
    }

    public ConfirmationDialog WithCanvasStrength(int radius) {
        _interactionBlocker.WithBlurRadius(radius);
        return this;
    }

    public ConfirmationDialog WithCornerRadius(CornerRadius radius) {
        PART_BorderRoot.CornerRadius = radius;
        PART_BorderHeader.CornerRadius = new CornerRadius(0, radius.BottomLeft);
        return this;
    }

    public ConfirmationDialog WithReverseButtons() {
        var children = PART_StackButtons.Children.ToList();
        children.Reverse();
        PART_StackButtons.Children.Clear();

        foreach (var child in children) {
            PART_StackButtons.Children.Add(child);
        }
        return this;
    }

    public ConfirmationDialog WithPasswordEntry(bool state = true, Predicate<string>? overrideValidation = null) {
        var passwordValidation = ServiceProvider.Get<IPasswordValidation>();
        if (passwordValidation.IsSet() is false) return this;
        _overrideValidation = overrideValidation;
        PART_PasswordEntryTip.Text = string.Empty;
        PART_PasswordEntry.IsVisible = state;
        PART_PasswordEntryTip.IsVisible = state;
        return this;
    }

    public ConfirmationDialog WithPasswordValidationFeedback(bool state = true) {
        if (state) {
            PART_PasswordEntry.TextChanged += PART_PasswordEntry_TextChanged;
        }
        else {
            PART_PasswordEntry.TextChanged -= PART_PasswordEntry_TextChanged;
        }
        return this;
    }

    private void PART_PasswordEntry_TextChanged(object? sender, TextChangedEventArgs e) {
        var passwordFormatValidation = ServiceProvider.Get<IPasswordFormatValidation>();
        passwordFormatValidation.IsFormatValid(PART_PasswordEntry.Text ?? string.Empty);
        PART_PasswordEntryTip.Text = passwordFormatValidation.Reason;
    }
}