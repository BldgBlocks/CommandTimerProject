using System;
using System.Linq;
using System.Security.Cryptography;

namespace CommandTimer.Desktop.Utilities;

/// <summary>
/// Simple access control for preventing accidental command execution.
/// This is a UI speed bump, not a security boundary.
/// The hash is stored in the user's config JSON alongside other settings.
/// A user with filesystem access can bypass this by editing the config — this is by design.
/// OS-level credential storage (keyring/keychain) was deliberately avoided to prevent
/// platform keyring dependencies, popups, and failures on minimal Linux installs.
/// </summary>
public class PasswordManager : IPasswordValidation, IPasswordFormatValidation {

    private ISerializer _serializer;

    private readonly IPasswordFormatValidation[] _formatValidators;

    private string[] _Reasons = [];
    public string[] Reasons => _Reasons;

    private const string DATA_KEY = "UserKey";
    private byte[]? _data;

    private void Serialize() {
        if (_data is not null) {
            _serializer.Serialize<byte[]>(DATA_KEY, _data, Settings.DEFAULT_DATA_FILE);
        }
    }
    private void Deserialize() {
        _data = _serializer.Deserialize<byte[]>(DATA_KEY, Settings.DEFAULT_DATA_FILE);
    }

    //...

    public PasswordManager(ISerializer serializer, IPasswordFormatValidation[] formatValidators) {
        _formatValidators = formatValidators;
        _serializer = serializer;
        Deserialize();
        /// Global Action
        ActionRelay.ActionPosted += (o, a) => {
            if (a.ActionKey == Settings.Keys.ActionRelay_Serialization) {
                Serialize();
            }
        };
    }

    private static (byte[] Salt, byte[] Hash) Encrypt(string password) {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        return (salt, hash);
    }


    //... Interface Implementation

    /// <summary>
    /// Check the password against the stored data.
    /// </summary>
    public bool Validate(string password) {
        if (_data is null) Deserialize();

        if (_data is null || _data.Length < 48) // Salt length + Hash length
            return false;

        var storedSaltLength = 16; // Length of the stored salt
        var storedSalt = new byte[storedSaltLength];
        Array.Copy(_data, storedSalt, storedSaltLength);

        byte[] trialHash = Rfc2898DeriveBytes.Pbkdf2(password, storedSalt, 100000, HashAlgorithmName.SHA256, 32);

        var storedHash = new byte[32];
        Array.Copy(_data, storedSaltLength, storedHash, 0, 32);

        return CryptographicOperations.FixedTimeEquals(trialHash, storedHash);
    }

    public void Store(string password) {
        var (salt, hash) = Encrypt(password);

        // Store both salt and hash in _data
        _data = new byte[salt.Length + hash.Length];
        Array.Copy(salt, 0, _data, 0, salt.Length);
        Array.Copy(hash, 0, _data, salt.Length, hash.Length);

        Serialize();
    }

    public bool IsFormatValid(string password) {
        _Reasons = _formatValidators
            .Where(v => !v.IsFormatValid(password))
            .Select(v => v.Reason)
            .ToArray();

        return _Reasons.Length == 0;
    }

    public bool IsSet() {
        var value = _serializer.Deserialize<byte[]>(DATA_KEY, Settings.DEFAULT_DATA_FILE);
        return value != null && value.Length != 0;
    }

    public string Reason => string.Join(", ", _Reasons);

}


