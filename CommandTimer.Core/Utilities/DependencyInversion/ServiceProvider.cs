using System;
using System.Collections.Generic;

namespace CommandTimer.Core.Utilities;

public static class ServiceProvider {

    private static readonly Dictionary<Type, object> _registry = [];

    public static T Get<T>() {
        if (_registry.TryGetValue(typeof(T), out var value)) {
            return (T)value;
        }
        throw new ArgumentException($"No service registered of type {typeof(T).Name}");
    }

    public static void Set<T>(T service) {
        if (service is null) return;
        if (typeof(T).IsInterface is false) {
            throw new ArgumentException("The provided type must be an interface.");
        }
        _registry[typeof(T)] = service;
    }
}
