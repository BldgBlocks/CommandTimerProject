namespace CommandTimer.Core.Static;

public class ObservableProperty<T>(T _value) {

    public virtual T Value {
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
    protected void OnValueChanged(T value) => ValueChanged?.Invoke(value);
    protected virtual T GetValue() {
        return _value ?? throw new InvalidOperationException("The static property must be initialized before use.");
    }
}





