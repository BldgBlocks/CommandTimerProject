using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommandTimer.Core.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CommandTimer.Core;
public static class IOUtils {

    /// <summary>
    /// Try to find a starting location to help the user navigate to wherever they want to get to first using
    /// what the consumer has currently, then cascading to a default path, and finally to a well known folder if needed.
    /// </summary>
    /// <param name="window">The window provides access to the <see cref="Window.StorageProvider"/> to try to find folders from the path.</param>
    /// <param name="desiredPath">The path that you have. If not correct, a series of defaults will be used.</param>
    /// <returns></returns>
    public static async Task<IStorageFolder?> GetFolderPickerStartingLocation(Window window, string desiredPath) {
        var folder = await window.StorageProvider.TryGetFolderFromPathAsync(desiredPath);
        if (folder != null) {
            var parentFolder = await GetParentFolderAsync(window, folder);
            if (parentFolder != null) {
                return parentFolder;
            }
        }

        return await window.StorageProvider.TryGetFolderFromPathAsync(Settings.DEFAULT_PATH) ??
               await window.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Desktop);
    }

    /// <summary>
    /// Get the parent folder, one level up.
    /// </summary>
    /// <returns>The folder modified to point one level up.</returns>
    public static async Task<IStorageFolder?> GetParentFolderAsync(Window window, IStorageFolder folder) {
        var parentFolderPath = System.IO.Path.GetDirectoryName(folder.Path.AbsolutePath);
        if (!string.IsNullOrEmpty(parentFolderPath)) {
            return await window.StorageProvider.TryGetFolderFromPathAsync(parentFolderPath);
        }
        return null;
    }

    /// <summary>
    /// Does the folder contain any items.
    /// </summary>
    /// <param name="folder">A folder location.</param>
    /// <returns><c>true</c> if empty, <c>false</c> if not empty.</returns>
    public static async Task<bool> IsFolderEmpty(IStorageFolder folder) {
        await foreach (IStorageItem item in folder.GetItemsAsync()) {
            return false;
        }
        return true;
    }

    public static FileInfo? GetMostRecentFile(string directoryPath, int depth) {
        return GetMostRecentFile(new DirectoryInfo(directoryPath), depth);
    }

    private static FileInfo? GetMostRecentFile(DirectoryInfo directory, int depth) {
        FileInfo? mostRecentFile = directory.GetFiles()
                                            .OrderByDescending(f => f.LastWriteTime)
                                            .FirstOrDefault();

        if (depth > 0) {
            var directories = directory.GetDirectories();
            foreach (var subDirectory in directories) {
                FileInfo? recentFileInSub = GetMostRecentFile(subDirectory, depth - 1);
                if (recentFileInSub != null && (mostRecentFile == null || recentFileInSub.LastWriteTime > mostRecentFile.LastWriteTime)) {
                    mostRecentFile = recentFileInSub;
                }
            }
        }

        return mostRecentFile;
    }

    /// <summary>
    /// Create the directory and file as necessary. If already exists, does nothing.
    /// </summary>
    public static void CreateFileAtPath(string directoryPath, string filePath) {
        try {
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }
            if (!File.Exists(filePath)) {
                File.WriteAllText(filePath, "");
            }
        }
        catch (UnauthorizedAccessException ex) {
            MessageRelay.OnMessagePosted("IO Utility", $"Access denied when trying to access or create directories/files: {ex.Message}", MessageRelay.MessageCategory.Exception);
        }
        catch (IOException ex) {
            MessageRelay.OnMessagePosted("IO Utility", $"An I/O error occurred: {ex.Message}", MessageRelay.MessageCategory.Exception);
        }
        catch (Exception ex) {
            MessageRelay.OnMessagePosted("IO Utility", $"Unexpected error during initialization: {ex.Message}", MessageRelay.MessageCategory.Exception);
        }
    }


    private static readonly SemaphoreSlim semaphore = new(1, 1);
    public static async Task AppendToFile(string path, string text) {
        if (File.Exists(path) is false) return;


        try {
            await semaphore.WaitAsync();

            using StreamWriter writer = new(path, true);

            await writer.WriteLineAsync(text);
        }
        catch (Exception ex) {
            MessageRelay.OnMessagePosted("Core", $"Failed to write to log: {ex.Message}", MessageRelay.MessageCategory.User, 0);
        }
        finally {
            semaphore.Release();
        }
    }

    public static void DeleteAllFilesInFolder(string folderPath) {
        try {
            if (Directory.Exists(folderPath)) {
                Directory.GetFiles(folderPath).ForEach(File.Delete);
            }
            else {
                Core.MessageRelay.OnMessagePosted(nameof(IOUtils), $"The directory '{folderPath}' was not found.", MessageRelay.MessageCategory.User);
            }
        }
        catch (UnauthorizedAccessException ex) {
            Core.MessageRelay.OnMessagePosted(nameof(IOUtils), $"Access to the path '{folderPath}' is denied. {ex.Message}", MessageRelay.MessageCategory.User);
        }
        catch (IOException ex) {
            Core.MessageRelay.OnMessagePosted(nameof(IOUtils), $"An I/O error occurred while deleting files: {ex.Message}", MessageRelay.MessageCategory.User);
        }
    }
}
