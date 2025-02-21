using Avalonia.Controls;
using Avalonia.Input;

namespace ControlsLibrary;

public class ShowEditableOnHover(Control target, StandardCursorType onHover = StandardCursorType.Hand) {
    public void Connect() {
        target.PointerEntered += Entered;
        target.PointerExited += Exited;
    }

    public void Disconnect() {
        target.PointerEntered -= Entered;
        target.PointerExited -= Exited;
    }

    public void Entered(object? sender, PointerEventArgs args) => target.Cursor = new Cursor(onHover);
    public void Exited(object? sender, PointerEventArgs args) => target.Cursor = new Cursor(StandardCursorType.Arrow);
}
