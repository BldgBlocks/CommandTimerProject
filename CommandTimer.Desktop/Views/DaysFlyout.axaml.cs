using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using CommandTimer.Core.Utilities;
using System;

namespace CommandTimer.Desktop.Views;

public partial class DaysFlyout : UserControl {

    private NumericUpDownBinding? binding;
    public NumericUpDown NumericUpDownControl;

    public DaysFlyout() {
        InitializeComponent();
        NumericUpDownControl = FlyoutDaysNumeric;
    }

    public NumericUpDownBinding Bind(FlyoutBase flyout, Button button) {
        if (binding is not null) throw new Exception("Binding in use, only one binding is allowed per instance.");

        binding = new NumericUpDownBinding(flyout, button, FlyoutDaysNumeric);
        binding.Connect();

        return binding;
    }

    public void Unbind() {
        binding?.Disconnect();
        binding = null;
    }

    private void Click_Accept(object? sender, RoutedEventArgs args) {
        binding?.OnAccept();
    }

    private void Click_Cancel(object? sender, RoutedEventArgs args) { 
        binding?.OnCancel();
    }

    private void Click_Clear(object? sender, RoutedEventArgs args) {
        NumericUpDownControl.Value = 0;
        binding?.OnAccept();
    }

}