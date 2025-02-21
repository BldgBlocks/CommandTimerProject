using CommandTimer.Core;
using CommandTimer.Core.Utilities.DependencyInversion;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace CommandTimer.Desktop.Utilities;
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
            if (a.ActionKey == Core.Settings.Keys.ActionRelay_Serialization) {
                Serialize();
            }
        };
    }

    private static (byte[] Salt, byte[] Hash) Encrypt(string password) {
        byte[] salt = new Byte[16];
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        var hash = deriveBytes.GetBytes(32);

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

        var deriveBytes = new Rfc2898DeriveBytes(password, storedSalt, 100000, HashAlgorithmName.SHA256);
        var trialHash = deriveBytes.GetBytes(32);

        return trialHash.SequenceEqual(_data.Skip(storedSaltLength).ToArray());
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
        var value = _serializer.Deserialize<byte[]>(DATA_KEY, Core.Settings.DEFAULT_DATA_FILE);
        return value != null && value.Length != 0;
    }

    public string Reason => string.Join(", ", _Reasons);

}
