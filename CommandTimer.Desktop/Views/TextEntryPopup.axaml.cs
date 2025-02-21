using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System;

namespace CommandTimer.Desktop.Views;

public partial class TextEntryPopup : UserControl {

    public TextEntryPopup() {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        Entry.BackgroundBorder!.BorderBrush = Core.Colors.ApplicationBrush_Accent;
        Entry.BackgroundBorder!.BorderThickness = new Avalonia.Thickness(1);

        base.OnLoaded(e);
    }

}