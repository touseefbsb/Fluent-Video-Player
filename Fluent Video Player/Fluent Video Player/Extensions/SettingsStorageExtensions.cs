﻿using Fluent_Video_Player.Core.Enums;
using Fluent_Video_Player.Core.Helpers;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Fluent_Video_Player.Extensions;

// Use these extension methods to store and retrieve local and roaming app data
// More details regarding storing and retrieving app data at https://docs.microsoft.com/windows/apps/design/app-settings/store-and-retrieve-app-data
public static class SettingsStorageExtensions
{
    private const string FileExtension = ".json";

    public static bool IsRoamingStorageAvailable(this ApplicationData appData) => appData.RoamingStorageQuota == 0;

    public static async Task SaveAsync<T>(this ApplicationDataContainer settings, string key, T value) => settings.SaveString(key, await Json.StringifyAsync(value));
    public static async Task SaveAsync<T>(this StorageFolder folder, string name, T content)
    {
        var file = await folder.CreateFileAsync(GetFileName(name), CreationCollisionOption.ReplaceExisting);
        var fileContent = await Json.StringifyAsync(content);

        await FileIO.WriteTextAsync(file, fileContent);
    }

    public static async Task<T?> ReadAsync<T>(this ApplicationDataContainer settings, string key)
    {
        if (settings.Values.TryGetValue(key, out var obj))
        {
            return await Json.ToObjectAsync<T>((string)obj);
        }
        return default;
    }
    public static async Task<T?> ReadAsync<T>(this StorageFolder folder, string name)
    {
        if (!File.Exists(Path.Combine(folder.Path, GetFileName(name))))
        {
            return default;
        }

        var file = await folder.GetFileAsync($"{name}.json");
        var fileContent = await FileIO.ReadTextAsync(file);

        return await Json.ToObjectAsync<T>(fileContent);
    }
    public static event EventHandler<System.EventArgs> OnCollectionViewSelected;
    public static void SaveString(this ApplicationDataContainer settings, string key, string value)
    {
        if (value == "GridView" || value == "ListView")
        {
            if (settings.Values.ContainsKey(key))
            {
                if (settings.Values[key].ToString() != value)
                {
                    settings.Values[key] = value;
                    OnCollectionViewSelected?.Invoke(null, System.EventArgs.Empty);
                }
            }
            else
            {
                settings.Values[key] = value;
                OnCollectionViewSelected?.Invoke(null, System.EventArgs.Empty);
            }
        }
        else
        {
            settings.Values[key] = value;
        }
    }

    public static async Task<StorageFile> SaveFileAsync(this StorageFolder folder, byte[] content, string fileName, CreationCollisionOption options = CreationCollisionOption.ReplaceExisting)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("File name is null or empty. Specify a valid file name", nameof(fileName));
        }

        var storageFile = await folder.CreateFileAsync(fileName, options);
        await FileIO.WriteBytesAsync(storageFile, content);
        return storageFile;
    }

    public static async Task<byte[]?> ReadFileAsync(this StorageFolder folder, string fileName)
    {
        var item = await folder.TryGetItemAsync(fileName).AsTask().ConfigureAwait(false);

        switch (item?.IsOfType(StorageItemTypes.File))
        {
            case true:
                {
                    var storageFile = await folder.GetFileAsync(fileName);
                    return await storageFile.ReadBytesAsync();
                }

            default:
                return null;
        }
    }

    public static async Task<byte[]?> ReadBytesAsync(this StorageFile file)
    {
        if (file != null)
        {
            using IRandomAccessStream stream = await file.OpenReadAsync();
            using var reader = new DataReader(stream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)stream.Size);
            var bytes = new byte[stream.Size];
            reader.ReadBytes(bytes);
            return bytes;
        }

        return null;
    }

    private static string GetFileName(string name) => string.Concat(name, FileExtension);

    #region Others
    public readonly static string CollectionViewKey = "CollectionView";
    public readonly static string DisplayModeKey = "DiplayMode";
    private static CurrentCollectionView currentCollectionView;
    private static CurrentDisplayMode currentDisplayMode;
    public static CurrentCollectionView ReadCurrentCollectionView(this ApplicationDataContainer settings)
    {
        if (settings.Values.ContainsKey(CollectionViewKey))
        {
            var view = settings.Values[CollectionViewKey].ToString();
            currentCollectionView = view == nameof(CurrentCollectionView.GridView) ? CurrentCollectionView.GridView : CurrentCollectionView.ListView;
        }
        else
        {
            settings.SaveString(CollectionViewKey, nameof(CurrentCollectionView.GridView));
            currentCollectionView = CurrentCollectionView.GridView;
        }
        return currentCollectionView;
    }
    public static CurrentDisplayMode ReadDisplayMode(this ApplicationDataContainer settings)
    {
        if (settings.Values.ContainsKey(DisplayModeKey))
        {
            var view = settings.Values[DisplayModeKey].ToString();
            currentDisplayMode = view == nameof(CurrentDisplayMode.LeftMode) ? CurrentDisplayMode.LeftMode : CurrentDisplayMode.TopMode;
        }
        else
        {
            settings.SaveString(DisplayModeKey, nameof(CurrentDisplayMode.LeftMode));
            currentDisplayMode = CurrentDisplayMode.LeftMode;
        }
        return currentDisplayMode;
    }
    #endregion Others
}
