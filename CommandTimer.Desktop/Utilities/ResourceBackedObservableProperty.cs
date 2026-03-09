using Avalonia.Media;
using CommandTimer.Core.Static.Exceptions;
using System;

namespace CommandTimer.Desktop.Utilities;

/// <summary>
/// ObservableProperty backed by Avalonia's Application.Current.Resources.
/// Reads and writes directly to the resource dictionary instead of holding an internal value.
/// Resources are the source of truth — getter always returns current resource value.
/// Special case: if resourceKey is null, acts as a constant observable (for ApplicationBrush_Transparent).
/// </summary>
public class ResourceBackedObservableProperty : ObservableProperty<AppColor> {

    private readonly string? _resourceKey;
    private readonly AppColor? _constantValue;

    public ResourceBackedObservableProperty(string resourceKeyName) : base(new AppColor()) {
        _resourceKey = resourceKeyName;
    }

    public ResourceBackedObservableProperty(AppColor constantValue) : base(constantValue) {
        _constantValue = constantValue;
    }

    protected override AppColor GetValue() {
        if (_constantValue is not null) return _constantValue.Value;

        if (_resourceKey is null) throw new InvalidOperationException("ResourceBackedObservableProperty has neither resourceKey nor constantValue.");

        if (Application.Current is not Application current)
            throw new Exceptions.ApplicationException($"Application not found when accessing resource '{_resourceKey}'.");

        var found = current.Resources.TryGetResource(_resourceKey, current.ActualThemeVariant, out var value);

        if (found is false) {
            MessageRelay.OnMessagePosted(this, $"ResourceBackedObservableProperty: Key '{_resourceKey}' not found. Theme: {current.ActualThemeVariant}. Returning purple fallback.", MessageRelay.MessageCategory.Exception, 100);
            return new AppColor(128, 0, 128, 255); // Purple fallback
        }

        if (value is not SolidColorBrush brush) {
            MessageRelay.OnMessagePosted(this, $"ResourceBackedObservableProperty: Key '{_resourceKey}' found but is type '{value?.GetType().Name}', not SolidColorBrush. Returning purple fallback.", MessageRelay.MessageCategory.Exception, 100);
            return new AppColor(128, 0, 128, 255); // Purple fallback
        }

        return new AppColor(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
    }

    public override AppColor Value {
        set {
            if (_constantValue is not null) return;
            if (_resourceKey is null) throw new InvalidOperationException("Cannot set value on constant ResourceBackedObservableProperty.");

            if (Application.Current is not Application current)
                throw new Exceptions.ApplicationException($"Application not found.");

            var currentColor = GetValue();
            if (currentColor == value) return;

            var brush = new SolidColorBrush(new Avalonia.Media.Color(value.A, value.R, value.G, value.B));
            current.Resources[_resourceKey] = brush;
            OnValueChanged(value);
        }
    }
}
