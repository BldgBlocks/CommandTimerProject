namespace CommandTimer.Core;

public interface ISerializer {

    public void Serialize<T>(string key, T value, string fileName);
    public T? Deserialize<T>(string key, string fileName);
    public bool Unserialize(string key, string fileName);
    public void Commit();
    public bool DeleteFile(string fileName);
    public void DeleteBackupFiles();
    public void DeleteBackupFiles(string rootName);
    public bool BackupFile(string fileName);
    public void RestoreFile(string fileName);

}
