using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CommandTimer.Core.Utilities;

public partial class CacheSerializer : ISerializer {

    //...

    private const string SERIALIZED_DATA_FILETYPE = ".json";
    private const string SERIALIZED_DATA_PATH = "Data";
    private const string JSON_BASE_TOKEN = "{ }";
    private readonly Dictionary<string, int> _changes = [];
    private readonly Dictionary<string, Dictionary<string, JsonElement>> _data = [];
    private string _workingDataPath = string.Empty;
    private string _appDirectoryPath;

    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Func<int> _getBackupVersions;
    private readonly Func<string> _getUserPath;
    private readonly Action<string> _postMessage;
    private readonly Action<ISerializer> _onCommit;

    public CacheSerializer(
        JsonSerializerOptions jsonOptions,
        Func<int> getBackupVersions,
        Func<string> getUserPath,
        Action<string> postMessage,
        Action<ISerializer> onCommit) {

        _jsonOptions = jsonOptions;
        _getBackupVersions = getBackupVersions;
        _getUserPath = getUserPath;
        _postMessage = postMessage;
        _onCommit = onCommit;

        _appDirectoryPath = Path.Combine(_getUserPath(), SERIALIZED_DATA_PATH);
        if (Directory.Exists(_appDirectoryPath) is false) {
            Directory.CreateDirectory(_appDirectoryPath);
        }
    }

    //... Data

    private void GetData(string filePath, out Dictionary<string, JsonElement> data) => data = GetData(filePath);
    private Dictionary<string, JsonElement> GetData(string filePath) {
        if (_data.TryGetValue(filePath, out var valueContents)) {
            return valueContents;
        }

        try {
            var existingJson = File.ReadAllText(filePath);
            valueContents = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(existingJson, _jsonOptions) ?? [];
            _data[filePath] = valueContents;
            return valueContents;
        }
        catch (FileNotFoundException) {
            valueContents = [];
            _data[filePath] = valueContents;
            return valueContents;
        }
        catch (JsonException jsonEx) {
            _postMessage($"Error deserializing existing data: {filePath} --- {jsonEx.Message}. Returning an empty dictionary.");
            valueContents = [];
            _data[filePath] = valueContents;
            return valueContents;
        }
        catch (IOException ex) {
            _postMessage($"I/O error when reading from {filePath}: {ex.Message}");
            throw;
        }
        catch (Exception ex) {
            _postMessage($"Unexpected error in GetData: {ex.Message}");
            throw;
        }
    }

    public void ClearAllSerializedData() {
        try {
            foreach (var kvp in _changes) {
                if (File.Exists(kvp.Key)) {
                    File.WriteAllText(kvp.Key, JSON_BASE_TOKEN);
                }
            }
            PurgeData();
        }
        catch (UnauthorizedAccessException ex) {
            _postMessage($"Access denied when trying to access or create directories/files: {ex.Message}");
        }
        catch (IOException ex) {
            _postMessage($"An I/O error occurred: {ex.Message}");
        }
        catch (Exception ex) {
            _postMessage($"Unexpected error during data clear: {ex.Message}");
        }
    }

    /// <summary>
    /// Purge a single data path.
    /// </summary>
    private void PurgeData(string dataPath) => _data[dataPath]?.Clear();
    /// <summary>
    /// Purge all data paths.
    /// </summary>
    private void PurgeData() {
        _data.Clear();
    }

    //... Interface Implementation

    /// <summary>
    /// Commit is not called by the serializer. Implement a strategy to call from the onCommit 
    /// Action argument passed in the constructor.
    /// </summary>
    public void Commit() {
        foreach (var kvp in _changes) {
            if (kvp.Value == 0) continue;
            try {
                _workingDataPath = kvp.Key;
                _changes[_workingDataPath] = 0;

                if (File.Exists(_workingDataPath) is false) {
                    CreateFileAtPath(_appDirectoryPath, _workingDataPath);
                    MakeFileJson(_workingDataPath);
                }

                GetData(_workingDataPath, out var data);
                File.WriteAllText(_workingDataPath, JsonSerializer.Serialize(data, _jsonOptions));
            }
            catch (IOException ex) {
                _postMessage($"Error writing to file {_workingDataPath}: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex) {
                _postMessage($"Access denied when trying to write to {_workingDataPath}: {ex.Message}");
            }
            catch (InvalidOperationException invalidEx) {
                _postMessage($"An unexpected error occurred during serialization: {invalidEx.Message}");
            }
            catch (Exception ex) {
                _postMessage($"An unexpected error occurred while serializing or writing to file: {ex.Message}");
            }
        }
    }

    public bool DeleteFile(string file)
        => DeleteFileAtPath(GetPathForFileNameWithExtension(file));

    private bool DeleteFileAtPath(string filePath) {
        try {
            if (File.Exists(filePath)) {
                File.Delete(filePath);
                PurgeData(filePath);
                return true;
            }
        }
        catch (Exception ex) {
            _postMessage($"Unable to delete file: ${filePath}, {ex.Message}");
            return false;
        }
        return false;
    }

    public void DeleteBackupFiles() {
        Directory.GetFiles(_appDirectoryPath)
                 .Where(IsBackupFile)
                 .ForEach(File.Delete);
    }

    public void DeleteBackupFiles(string rootName) {
        GetFilesWithRootName(rootName).Where(IsBackupFile)
                                      .ForEach(File.Delete);
    }

    public bool BackupFile(string file)
        => BackupFileAtPath(GetPathForFileNameWithExtension(file));

    private bool BackupFileAtPath(string filePath) {
        var backupVersions = _getBackupVersions();
        if (backupVersions <= 0) return false;
        if (File.Exists(filePath) is false) return false;
        var fileName = Path.GetFileNameWithoutExtension(filePath);

        for (int i = backupVersions - 1; i >= 1; i--) {
            string currentVersionPath = Path.Combine(_appDirectoryPath, AppendVersionNumber(GetBackupNameWithExtension(fileName), i));
            string nextVersionPath = Path.Combine(_appDirectoryPath, AppendVersionNumber(GetBackupNameWithExtension(fileName), i + 1));

            if (File.Exists(currentVersionPath)) {
                try {
                    File.Move(currentVersionPath, nextVersionPath, true);
                }
                catch (Exception) {
                    return false;
                }
            }
        }

        /// Move the current file to version 1
        File.Move(filePath, Path.Combine(_appDirectoryPath, AppendVersionNumber(GetBackupNameWithExtension(fileName), 1)));

        /// Remove extras
        CleanBackups(backupVersions, fileName);

        PurgeData(filePath);
        return true;
    }

    public void CleanBackups(int backupVersions, string fileName) {
        GetFilesWithRootName(fileName)
                    .Where(IsBackupFile)
                    .Where(fileName => ExtractVersionNumber(fileName) > backupVersions)
                    .ForEach(File.Delete);
    }

    public void RestoreFile(string fileName) {
        fileName = Path.GetFileNameWithoutExtension(fileName);
        var filePath = GetPathForFileNameWithExtension(fileName);
        PurgeData(filePath);

        var files = GetFilesWithRootName(fileName).Where(IsBackupFile)
                                                  .OrderBy(i => ExtractVersionNumber(fileName))
                                                  .ToArray();
        if (files.Length is 0) return;

        File.Move(files[0], filePath, true);
        for (int i = 2; i < files.Length; i++) {
            var backupName = GetBackupNameWithExtension(fileName);
            var versioning = AppendVersionNumber(backupName, i - 1);
            var fullPath = GetPathForFileName(versioning);
            File.Move(files[i], fullPath, true);
            /// Reload data
            GetData(filePath);
        }
    }

    public void Serialize<T>(string key, T value, string fileName) {
        string filePath = GetPathForFileNameWithExtension(fileName);
        string jsonText;

        _changes[filePath] = _changes.TryGetValue(filePath, out int count) ? ++count : 1;

        try {
            var data = GetData(filePath);
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(value, typeof(T), _jsonOptions));

            jsonText = doc.RootElement.GetRawText();
            data[key] = JsonDocument.Parse(jsonText).RootElement;

            _onCommit(this);
        }
        catch (JsonException jsonEx) {
            _postMessage($"Error serializing object of type {typeof(T).Name}: {jsonEx.Message}");
        }
        catch (InvalidOperationException invalidEx) {
            _postMessage($"An unexpected error occurred during serialization: {invalidEx.Message}");
        }
        catch (Exception ex) {
            _postMessage($"An unexpected error occurred during serialization: {ex.Message}");
        }
    }

    public T? Deserialize<T>(string key, string fileName) {
        var filePath = GetPathForFileNameWithExtension(fileName);
        var data = GetData(filePath);

        if (data.TryGetValue(key, out JsonElement element)) {
            try {
                return JsonSerializer.Deserialize<T>(element.GetRawText(), _jsonOptions);
            }
            catch (JsonException jsonEx) {
                _postMessage($"Error deserializing object of type {typeof(T).Name} for key '{key}': {jsonEx.Message}");
            }
            catch (InvalidOperationException ex) {
                _postMessage($"Invalid operation error when deserializing: {ex.Message}");
            }
            catch (Exception ex) {
                _postMessage($"Unexpected error during deserialization: {ex.Message}");
            }
        }
        return default;
    }

    public bool Unserialize(string key, string fileName) {
        var filePath = GetPathForFileNameWithExtension(fileName);
        var data = GetData(filePath);

        if (data.Remove(key)) {
            _changes[filePath]++;
            _onCommit(this);
            return true;
        }
        return false;
    }

    private void CreateFileAtPath(string directoryPath, string filePath) {
        try {
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }
            if (!File.Exists(filePath)) {
                File.WriteAllText(filePath, string.Empty);
            }
        }
        catch (UnauthorizedAccessException ex) {
            _postMessage($"Access denied when trying to access or create directories/files: {ex.Message}");
        }
        catch (IOException ex) {
            _postMessage($"An I/O error occurred: {ex.Message}");
        }
        catch (Exception ex) {
            _postMessage($"Unexpected error during initialization: {ex.Message}");
        }
    }

    private string GetPathForFileNameWithExtension(string fileName)
        => Path.Combine(_appDirectoryPath, $"{Path.GetFileNameWithoutExtension(fileName)}{SERIALIZED_DATA_FILETYPE}");

    private string GetPathForFileName(string fileName)
        => Path.Combine(_appDirectoryPath, fileName);

    private void MakeFileJson(string filePath) {
        try {
            if (File.Exists(filePath)) {
                using StreamReader reader = new(filePath);
                if (reader.ReadLine()?.Contains('{') is false) {
                    File.WriteAllText(filePath, JSON_BASE_TOKEN);
                }
            }
            else {
                throw new FileNotFoundException(filePath);
            }
        }
        catch (Exception ex) {
            _postMessage($"Error creating file. {ex.Message}");
        }
    }

    private bool IsOfRootName(string file, string rootName) {
        if (Path.GetFileName(file).Contains('.')) {
            var splitAllDotsOff = Path.GetFileName(file).Split('.')[0];
            return splitAllDotsOff.Equals(rootName, StringComparison.Ordinal);
        }
        else {
            return Path.GetFileName(file).Equals(rootName, StringComparison.Ordinal);
        }
    }

    private IEnumerable<string> GetFilesWithRootName(string rootName)
        => Directory.GetFiles(_appDirectoryPath)
                    .Where(f => IsOfRootName(f, rootName));

    private bool IsBackupFile(string fileName)
        => Path.GetExtension(fileName)
               .Contains(".bak", StringComparison.Ordinal);

    private string GetBackupNameWithExtension(string fileName) {
        if (fileName.Contains('.')) {
            fileName = fileName.Split('.')[0];
        }
        return $"{fileName}{SERIALIZED_DATA_FILETYPE}.bak";
    }

    private string RemoveBackupExtension(string file) {
        Match match = DecodeVersionNumberRegex().Match(file);
        if (match.Success && match.Groups[1].Success) {
            // Peel off outer file extension abc.json.bak(1) => abc.json
            return Path.GetFileNameWithoutExtension(file);
        }
        return file;
    }

    private string AppendVersionNumber(string file, int version)
        => $"{file}({version})";

    private int ExtractVersionNumber(string fileName) {
        Match match = DecodeVersionNumberRegex().Match(fileName);
        if (match.Success && match.Groups[1].Success) {
            return int.Parse(match.Groups[1].Value);
        }
        return -1;
    }

    [GeneratedRegex(@".{0,3}\((\d{1,2})\)")]
    private static partial Regex DecodeVersionNumberRegex();
}
