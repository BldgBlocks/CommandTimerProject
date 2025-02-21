using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using System;
using Avalonia.Controls.Primitives;

namespace ControlsLibrary;

/// <summary>
/// Implements IDisposable for the rendering of a ghost. The rendering should already be disposed based on
/// pointer actions. However disposal is available to be sure at the end of its life cycle.
/// </summary>
public class ListItemDragHandler {

    private readonly Control _handle;
    private readonly UserControl _activeView;
    private readonly Panel _itemView;
    private readonly DataObject _data;
    private bool _isPointerDown;
    private bool _isDragDropActive;
    private Point _initialPosition;
    private RenderTargetBitmap? _disposableBitmap;
    public static readonly int DRAG_THRESHOLD = 10;

    public int DragPadding { get; set; } = 25;
    public Popup Ghost { get; private set; }
    public Border GhostContent { get; private set; }

    public ListItemDragHandler(Control handle, Panel item, UserControl activeArea, DataObject data) {
        _handle = handle;
        _activeView = activeArea;
        _itemView = item;
        _data = data;
        Ghost = new Popup() { Name = "Ghost" };
        GhostContent = new Border { Name = "GhostContent" };
        Ghost.Child = GhostContent;
        _itemView.Children.Insert(0, Ghost);
    }

    public void Connect() {
        _handle.PointerPressed += Handle_PointerPressed;
        _handle.PointerMoved += Handle_PointerMoved;
        _handle.PointerReleased += Handle_PointerReleased;
        _handle.PointerEntered += Handle_PointerEntered;
        _handle.PointerExited += Handle_PointerExited;
    }

    public void Disconnect() {
        _handle.PointerPressed -= Handle_PointerPressed;
        _handle.PointerMoved -= Handle_PointerMoved;
        _handle.PointerReleased -= Handle_PointerReleased;
        _handle.PointerEntered -= Handle_PointerEntered;
        _handle.PointerExited -= Handle_PointerExited;
        Unsubscribe();
    }

    private void Handle_PointerExited(object? sender, PointerEventArgs e) {
        _handle.Cursor = new Cursor(StandardCursorType.Arrow);
    }

    private void Handle_PointerEntered(object? sender, PointerEventArgs e) {
        _handle.Cursor = new Cursor(StandardCursorType.Hand);
    }

    private void Handle_PointerPressed(object? sender, PointerPressedEventArgs e) {
        /// Gate, Once per click
        if (_isPointerDown) return;
        _isPointerDown = true;

        _initialPosition = e.GetPosition(_handle);
    }

    private void Handle_PointerMoved(object? sender, PointerEventArgs e) {
        if (_isPointerDown is false) return;

        var currentPosition = e.GetPosition(_itemView);

        if (_isDragDropActive) {
            HandleActiveDrag(e, currentPosition);
            return;
        }

        _isDragDropActive = CheckInitialMoveThresholds(currentPosition);
        if (!_isDragDropActive) return;
        ShowGhostItem();
    }

    private void Handle_PointerReleased(object? sender, PointerReleasedEventArgs e) {
        try {
            if (IsWithinDragPadding(e.GetPosition(_activeView), _activeView)) {
                /// Reset
                _isDragDropActive = false;
                _isPointerDown = false;
                HideGhost();
                InitiateDragOperation(e);
            }
        }
        finally {
            HideGhost();

            Unsubscribe();
        }
    }

    public void Unsubscribe() {
        _disposableBitmap?.Dispose();
        _disposableBitmap = null;
    }

    //...

    private void HandleActiveDrag(PointerEventArgs e, Point currentPosition) {
        var positionInActiveView = e.GetPosition(_activeView);

        if (IsWithinDragPadding(positionInActiveView, _activeView)) {
            UpdateGhostPosition(currentPosition);
        }
    }

    private bool IsWithinDragPadding(Point positionInActiveView, UserControl activeView) {
        return positionInActiveView.Y > activeView.Bounds.Top + DragPadding &&
               positionInActiveView.Y < activeView.Bounds.Bottom - DragPadding * 2;
    }

    private bool CheckInitialMoveThresholds(Point currentPosition) {
        return Math.Abs(currentPosition.X - _initialPosition.X) >= DRAG_THRESHOLD
            || Math.Abs(currentPosition.Y - _initialPosition.Y) >= DRAG_THRESHOLD;
    }

    private void ShowGhostItem() {
        SetCursor(StandardCursorType.DragMove);
        GhostContent.Child = RenderToImage(_itemView);
        Ghost.Placement = PlacementMode.Pointer;
        Ghost.Open();

        /// After render
        _itemView.Opacity = 0.5;
    }

    private void SetCursor(StandardCursorType cursorType) => _handle.Cursor = new Cursor(cursorType);

    private Image RenderToImage(Control target) {
        var image = new Image();

        var width = target.Bounds.Width;
        var height = target.Bounds.Height;
        if (width <= 1 || height <= 1) return image;

        Unsubscribe();
        _disposableBitmap = new RenderTargetBitmap(new PixelSize((int)width, (int)height), new Vector(96, 96));
        _disposableBitmap.Render(target);
        image.Source = _disposableBitmap;
        image.Width = width;
        image.Height = height;

        return image;
    }

    private void HideGhost() {
        SetCursor(StandardCursorType.Arrow);
        Ghost.Close();
        _itemView.Opacity = 1;
    }

    private void UpdateGhostPosition(Point currentPosition) {
        if (_isDragDropActive) {
            Ghost.HorizontalOffset = -currentPosition.X;
            Ghost.VerticalOffset = currentPosition.Y * 0.08 - (_itemView.Bounds.Height / 2);
        }
    }

    private void InitiateDragOperation(PointerReleasedEventArgs e) {
        DragDrop.DoDragDrop(e, _data, DragDropEffects.Move);
    }
}
