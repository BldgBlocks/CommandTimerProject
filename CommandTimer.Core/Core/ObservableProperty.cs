using System;
using System.Collections.Generic;

namespace CommandTimer.Core;

public class ObservableProperty<T>(T _value) {

    public T Value {
        get {
            return GetValue();
        }
        set {
            if (value == null) {
                throw new ArgumentNullException(nameof(value), "Value cannot be set to null.");
            }
            if (EqualityComparer<T>.Default.Equals(_value, value) is false) {
                _value = value;
                OnValueChanged(value);
            }
        }
    }

    public event Action<T>? ValueChanged;
    private void OnValueChanged(T value) => ValueChanged?.Invoke(value);
    public T GetValue() {
        return _value ?? throw new InvalidOperationException("The static property must be initialized before use.");
    }
}


