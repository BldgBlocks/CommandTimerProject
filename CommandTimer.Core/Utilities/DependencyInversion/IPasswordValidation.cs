namespace CommandTimer.Core.Utilities.DependencyInversion;

public interface IPasswordValidation {
    public bool IsSet();
    public bool Validate(string password);
    public void Store(string password);
}

public interface IPasswordFormatValidation {
    public bool IsFormatValid(string password);
    public string Reason { get; }
}
