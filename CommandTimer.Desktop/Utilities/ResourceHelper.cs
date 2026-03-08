using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandTimer.Desktop.Utilities;

/// <summary>
/// Fail-fast resource lookup helper for Avalonia Application.Current.Resources.
/// Throws exceptions instead of returning defaults when resources are missing or mistyped.
/// Prevents silent configuration errors.
/// </summary>
public static class ResourceHelper {

    /// <summary>
    /// Gets a typed resource from Application.Current.Resources or throws.
    /// </summary>
    /// <typeparam name="T">The expected resource type.</typeparam>
    /// <param name="resourceKey">The resource key to lookup.</param>
    /// <returns>The typed resource value.</returns>
    /// <exception cref="InvalidOperationException">Application.Current not initialized.</exception>
    /// <exception cref="KeyNotFoundException">Resource key not found.</exception>
    /// <exception cref="InvalidCastException">Resource found but wrong type.</exception>
    public static T GetResourceOrThrow<T>(string resourceKey) {
        if (Application.Current is not Application current)
            throw new InvalidOperationException($"Application.Current not initialized when accessing resource '{resourceKey}'.");

        var found = current.Resources.TryGetResource(resourceKey, current.ActualThemeVariant, out var value);

        if (found is false) {
            var availableKeys = string.Join(", ", current.Resources.Keys.Cast<object>().Select(k => k.ToString()).Take(20));
            throw new KeyNotFoundException($"Resource '{resourceKey}' not found in Application.Current.Resources. Theme: {current.ActualThemeVariant}. Available keys (first 20): {availableKeys}");
        }

        if (value is not T typedValue)
            throw new InvalidCastException($"Resource '{resourceKey}' found but is type '{value?.GetType().Name}', expected '{typeof(T).Name}'.");

        return typedValue;
    }
}
