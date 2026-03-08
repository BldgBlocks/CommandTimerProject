using Avalonia.Controls;

namespace CommandTimer.Core.Interfaces;

public interface IShowToolTip {
    public void OnPointerOver(Control control, string tooltip, IShowToolTipProperties properties);
    public void Show();
    public void Hide();
    public void Enable();
    public void Disable();
    public bool IsOpen();
}

