using Fluent_Video_Player.Contracts.Services;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage;
using Fluent_Video_Player.Helpers;

namespace Fluent_Video_Player.Services
{
    internal class StorageFileService : IStorageFileService
    {
        #region Fields 
        readonly string videoFilter = "System.Kind:=System.Kind#Video ext:<>.srt";
        public List<string> VideoFileTypes => new()
                                           { ".webm", ".flv", ".mkv", ".vob", ".ogv", ".ogg",
                                             ".drc", ".gif", ".gifv", ".mng", ".avi",".mov",
                                             ".qt",".wmv",".yuv",".rm",".rmvb",".asf",".amv",
                                             ".mp4",".m4p",".m4v",".mpg",".mp2",".mpeg",".mpe",
                                             ".mpv",".m2v",".svi",".3gp",".3g2",".mxf",
                                             ".roq",".nsv",".f4v",".f4p",".f4a",".f4b",".divx"};

        internal readonly string[] RequiredVideoProperties = new string[] { "System.Media.Duration" };
        private QueryOptions videoFileOptions;
        private QueryOptions videoFolderOptions;
        #endregion Fields 

        #region Methods
        public async Task<string> GetDurationAsync(StorageFile file)
        {
            var size = await file.Properties.RetrievePropertiesAsync(RequiredVideoProperties);
            var ticks = Convert.ToInt64(size["System.Media.Duration"]);
            return TimeSpan.FromTicks(ticks).ToString(@"hh\:mm\:ss");
        }

        public void AddToHistory(StorageFile fileToBeAdded)
        {
            if (StorageApplicationPermissions.FutureAccessList.Entries.Count >= 999)
            {
                var tokenToRemove = StorageApplicationPermissions.FutureAccessList.Entries[^1].Token; // last item
                StorageApplicationPermissions.FutureAccessList.Remove(tokenToRemove);
            }
            StorageApplicationPermissions.FutureAccessList.Add(fileToBeAdded);
        }

        public async Task<BitmapImage?> GetDisplayForFileAsync(StorageFile MyVideoFile)
        {
            if (MyVideoFile is null)
            {
                return null;
            }
            var bitm = new BitmapImage();
            using var imgSource = await MyVideoFile.GetScaledImageAsThumbnailAsync(ThumbnailMode.VideosView, Constants._thumbnailReqestedSize, ThumbnailOptions.UseCurrentScale);
            if (imgSource != null)
            {
                await bitm.SetSourceAsync(imgSource); return bitm;
            }
            return null;
        }

        public StorageFileQueryResult GetVideoFilesQuery(StorageFolder Folder, uint thumbnailRequestedSize)
        {
            if (videoFileOptions is default(QueryOptions))
            {
                videoFileOptions = new QueryOptions()
                {
                    IndexerOption = IndexerOption.UseIndexerWhenAvailable,
                };
                foreach (var filter in VideoFileTypes)
                {
                    videoFileOptions.FileTypeFilter.Add(filter);
                }
                videoFileOptions.ApplicationSearchFilter += videoFilter;
                //get basic properties as well if needed
                videoFileOptions.SetPropertyPrefetch(PropertyPrefetchOptions.VideoProperties, RequiredVideoProperties);
                videoFileOptions.SetThumbnailPrefetch(ThumbnailMode.VideosView, thumbnailRequestedSize, ThumbnailOptions.UseCurrentScale);
            }
            return Folder.CreateFileQueryWithOptions(videoFileOptions);
        }

        public StorageFolderQueryResult GetVideoFoldersQuery(StorageFolder Folder)
        {
            if (videoFolderOptions is default(QueryOptions))
            {
                videoFolderOptions = new QueryOptions(CommonFolderQuery.DefaultQuery)
                {
                    IndexerOption = IndexerOption.UseIndexerWhenAvailable,
                };
            }
            return Folder.CreateFolderQueryWithOptions(videoFolderOptions);
        }
        #endregion Methods
    }
}
