using Avalonia;
using Avalonia.Media;
using CommandTimer.Core.Static.Exceptions;

namespace CommandTimer.Core.Utilities;

/// <summary>
/// ObservableProperty backed by Avalonia's Application.Current.Resources.
/// Reads and writes directly to the resource dictionary instead of holding an internal value.
/// Resources are the source of truth — getter always returns current resource value.
/// Special case: if resourceKey is null, acts as a constant observable (for ApplicationBrush_Transparent).
/// </summary>
public class ResourceBackedObservableProperty {

    private readonly string? _resourceKey;
    private readonly SolidColorBrush? _constantValue;

    public ResourceBackedObservableProperty(string resourceKey) {
        _resourceKey = resourceKey;
    }

    public ResourceBackedObservableProperty(SolidColorBrush constantValue) {
        _constantValue = constantValue;
    }

    public event Action<SolidColorBrush>? ValueChanged;

    public SolidColorBrush Value {
        get {
            if (_constantValue is not null) return _constantValue;
            if (_resourceKey is null) throw new InvalidOperationException("ResourceBackedObservableProperty has neither resourceKey nor constantValue.");

            if (Application.Current is not Application current)
                throw new Exceptions.ApplicationException($"Application not found when accessing resource '{_resourceKey}'.");

            var found = current.Resources.TryGetResource(_resourceKey, current.ActualThemeVariant, out var value);

            if (found is false) {
                MessageRelay.OnMessagePosted(this, $"ResourceBackedObservableProperty: Key '{_resourceKey}' not found. Theme: {current.ActualThemeVariant}. Returning purple fallback.", MessageRelay.MessageCategory.Exception, 100);
                return new SolidColorBrush(Avalonia.Media.Colors.Purple);
            }

            if (value is not SolidColorBrush brush) {
                MessageRelay.OnMessagePosted(this, $"ResourceBackedObservableProperty: Key '{_resourceKey}' found but is type '{value?.GetType().Name}', not SolidColorBrush. Returning purple fallback.", MessageRelay.MessageCategory.Exception, 100);
                return new SolidColorBrush(Avalonia.Media.Colors.Purple);
            }

            return brush;
        }
        set {
            if (_constantValue is not null) return;
            if (_resourceKey is null) throw new InvalidOperationException("Cannot set value on constant ResourceBackedObservableProperty.");

            if (Application.Current is not Application current)
                throw new Exceptions.ApplicationException($"Application not found.");

            var currentBrush = Value;
            if (currentBrush.Color == value.Color) return;

            current.Resources[_resourceKey] = value;
            ValueChanged?.Invoke(value);
        }
    }
}

