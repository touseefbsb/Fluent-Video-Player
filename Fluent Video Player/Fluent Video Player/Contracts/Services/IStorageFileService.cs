using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Search;

namespace Fluent_Video_Player.Contracts.Services
{
    public interface IStorageFileService
    {
        List<string> VideoFileTypes { get; }
        void AddToHistory(StorageFile fileToBeAdded);
        Task<BitmapImage?> GetDisplayForFileAsync(StorageFile MyVideoFile);
        Task<string> GetDurationAsync(StorageFile file);
        StorageFileQueryResult GetVideoFilesQuery(StorageFolder Folder, uint thumbnailRequestedSize);
        StorageFolderQueryResult GetVideoFoldersQuery(StorageFolder Folder);
    }
}
