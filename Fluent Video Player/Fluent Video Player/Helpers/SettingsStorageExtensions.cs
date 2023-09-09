using System;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Streams;

namespace Fluent_Video_Player.Helpers
{
    // Use these extension methods to store and retrieve local and roaming app data
    // More details regarding storing and retrieving app data at https://docs.microsoft.com/windows/uwp/app-settings/store-and-retrieve-app-data
    public static class SettingsStorageExtensions
    {
        private const string FileExtension = ".json";

        public static bool IsRoamingStorageAvailable(this ApplicationData appData)
        {
            return appData.RoamingStorageQuota == 0;
        }

        public static async Task SaveAsync<T>(this StorageFolder folder, string name, T content)
        {
            var file = await folder.CreateFileAsync(GetFileName(name), CreationCollisionOption.ReplaceExisting);
            var fileContent = await Json.StringifyAsync(content);

            await FileIO.WriteTextAsync(file, fileContent);
        }

        public static async Task<T> ReadAsync<T>(this StorageFolder folder, string name)
        {
            if (!File.Exists(Path.Combine(folder.Path, GetFileName(name))))
            {
                return default(T);
            }

            var file = await folder.GetFileAsync($"{name}.json");
            var fileContent = await FileIO.ReadTextAsync(file);

            return await Json.ToObjectAsync<T>(fileContent);
        }

        public static async Task SaveAsync<T>(this ApplicationDataContainer settings, string key, T value)
        {
            settings.SaveString(key, await Json.StringifyAsync(value));
        }
        public static event EventHandler<EventArgs> OnCollectionViewSelected;
        public static void SaveString(this ApplicationDataContainer settings, string key, string value)
        {
            if (value == "GridView" || value == "ListView")
            {
                if (settings.Values.ContainsKey(key))
                {
                    if (settings.Values[key].ToString() != value)
                    {
                        settings.Values[key] = value;
                        OnCollectionViewSelected?.Invoke(null, EventArgs.Empty);
                    }
                }
                else
                {
                    settings.Values[key] = value;
                    OnCollectionViewSelected?.Invoke(null, EventArgs.Empty);
                }
            }
            else
            {
                settings.Values[key] = value;
            }
        }
        public readonly static string CollectionViewKey = "CollectionView";
        public readonly static string DisplayModeKey = "DiplayMode";
        private static CurrentCollectionView currentCollectionView;
        private static CurrentDisplayMode currentDisplayMode;
        public static CurrentCollectionView ReadCurrentCollectionView(this ApplicationDataContainer settings)
        {
            if (settings.Values.ContainsKey(CollectionViewKey))
            {
                string view = settings.Values[CollectionViewKey].ToString();
                if (view == CurrentCollectionView.GridView.ToString())
                    currentCollectionView = CurrentCollectionView.GridView;
                else
                    currentCollectionView = CurrentCollectionView.ListView;
            }
            else
            {
                settings.SaveString(CollectionViewKey, CurrentCollectionView.GridView.ToString());
                currentCollectionView = CurrentCollectionView.GridView;
            }
            return currentCollectionView;
        }
        public static CurrentDisplayMode ReadDisplayMode(this ApplicationDataContainer settings)
        {
            if (settings.Values.ContainsKey(DisplayModeKey))
            {
                string view = settings.Values[DisplayModeKey].ToString();
                if (view == CurrentDisplayMode.LeftMode.ToString())
                    currentDisplayMode = CurrentDisplayMode.LeftMode;
                else
                    currentDisplayMode = CurrentDisplayMode.TopMode;
            }
            else
            {
                settings.SaveString(DisplayModeKey, CurrentDisplayMode.LeftMode.ToString());
                currentDisplayMode = CurrentDisplayMode.LeftMode;
            }
            return currentDisplayMode;
        }
        public static string ReadString(this ApplicationDataContainer settings, string key) => settings.Values[key].ToString();
        public static async Task<T> ReadAsync<T>(this ApplicationDataContainer settings, string key)
        {
            object obj = null;

            if (settings.Values.TryGetValue(key, out obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }

            return default(T);
        }

        public static async Task<StorageFile> SaveFileAsync(this StorageFolder folder, byte[] content, string fileName, CreationCollisionOption options = CreationCollisionOption.ReplaceExisting)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("ExceptionSettingsStorageExtensionsFileNameIsNullOrEmpty".GetLocalized(), nameof(fileName));
            }

            var storageFile = await folder.CreateFileAsync(fileName, options);
            await FileIO.WriteBytesAsync(storageFile, content);
            return storageFile;
        }

        public static async Task<byte[]> ReadFileAsync(this StorageFolder folder, string fileName)
        {
            var item = await folder.TryGetItemAsync(fileName).AsTask().ConfigureAwait(false);

            if ((item != null) && item.IsOfType(StorageItemTypes.File))
            {
                var storageFile = await folder.GetFileAsync(fileName);
                byte[] content = await storageFile.ReadBytesAsync();
                return content;
            }

            return null;
        }

        public static async Task<byte[]> ReadBytesAsync(this StorageFile file)
        {
            if (file != null)
            {
                using (IRandomAccessStream stream = await file.OpenReadAsync())
                {
                    using (var reader = new DataReader(stream.GetInputStreamAt(0)))
                    {
                        await reader.LoadAsync((uint)stream.Size);
                        var bytes = new byte[stream.Size];
                        reader.ReadBytes(bytes);
                        return bytes;
                    }
                }
            }

            return null;
        }

        private static string GetFileName(string name)
        {
            return string.Concat(name, FileExtension);
        }
    }
    public enum CurrentCollectionView { GridView, ListView }
    public enum CurrentDisplayMode { LeftMode, TopMode }
}
