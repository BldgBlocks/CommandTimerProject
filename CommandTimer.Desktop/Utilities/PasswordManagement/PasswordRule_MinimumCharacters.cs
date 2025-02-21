using CommandTimer.Core.Utilities.DependencyInversion;

namespace CommandTimer.Desktop.Utilities;
public class PasswordRule_MinimumCharacters(int MinimumCharacters) : IPasswordFormatValidation {

    private string _Reason = string.Empty;
    public string Reason => _Reason;

    public bool IsFormatValid(string password) {
        var decision = password.Length >= MinimumCharacters;
        _Reason = decision ? string.Empty : $"Must contain {MinimumCharacters} characters";
        return decision;
    }
}
