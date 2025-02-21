using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CommandTimer.Desktop.Views;

public partial class ConfirmationWindow : Window {

    //... 

    /// <summary>
    /// Returns a Task that doesn't await or return immediately, and can be interacted with by other methods,
    /// such as a button setting a state to return through the task.
    /// </summary>
    private readonly TaskCompletionSource<bool?> _tcs = new();

    /// <summary>
    /// Blocks interaction to other areas of the application.
    /// </summary>
    public new Task<bool?> ShowDialog(Window window) {
        base.ShowDialog(window);
        return _tcs.Task;
    }

    public new Task<bool?> Show(Window window) {
        base.Show(window);
        return _tcs.Task;
    }

    public new void Close() {
        base.Close();  
    }

    public ConfirmationWindow() {
        InitializeComponent();
    }

    private void Tapped_ButtonYes(object sender, TappedEventArgs e) {
        _tcs.TrySetResult(true);
        Close();
    }

    private void Tapped_ButtonNo(object sender, TappedEventArgs e) {
        _tcs.TrySetResult(false);
        Close();
    }

    private void Tapped_ButtonCancel(object sender, TappedEventArgs e) {
        _tcs.TrySetResult(null);
        Close();
    }

    //... Builder Methods

    public static ConfirmationWindow Create() => new();

    public ConfirmationWindow WithTitle(string title) {
        PART_TextTitle.Text = title;
        return this;
    }

    public ConfirmationWindow WithMessage(string message) {
        PART_TextMessage.Text = message;
        return this;
    }

    public ConfirmationWindow WithChoiceYesText(string message) {
        PART_ButtonYes.Content = message;
        return this;
    }

    public ConfirmationWindow WithChoiceNoText(string message) {
        PART_ButtonNo.Content = message;
        return this;
    }

    public ConfirmationWindow WithChoiceCancelText(string message) {
        PART_ButtonCancel.Content = message;
        return this;
    }

    public ConfirmationWindow WithCancelButton(bool state) {
        PART_ButtonCancel.IsVisible = state;
        return this;
    }

    public ConfirmationWindow WithMaxSize(int width, int height) {
        this.MaxWidth = Math.Max(this.MinWidth, width);
        this.MaxHeight = Math.Max(this.MinHeight, height);
        return this;
    }

    public ConfirmationWindow WithMinSize(int width, int height) {
        this.MinWidth = Math.Clamp(width, 200, this.MaxWidth);
        this.MinHeight = Math.Clamp(height, 200, this.MaxHeight);
        return this;
    }

    public ConfirmationWindow WithBorder(IBrush brush, Thickness thickness) {
        PART_BorderRoot.BorderBrush = brush;
        PART_BorderRoot.BorderThickness = thickness;
        return this;
    }

    public ConfirmationWindow WithBackground(IBrush brush) { 
        PART_BorderRoot.Background = brush;
        return this;
    }

    public ConfirmationWindow WithReverseButtons() {
        var children = PART_StackButtons.Children.ToList();
        children.Reverse();
        PART_StackButtons.Children.Clear();

        foreach (var child in children) {
            PART_StackButtons.Children.Add(child);
        }
        return this;
    }
}