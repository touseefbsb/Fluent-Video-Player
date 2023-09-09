using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Services.Store;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Xaml.Media.Imaging;

namespace Fluent_Video_Player.Helpers
{
    internal static class FileHelper
    {
        #region StaticFields 
        static readonly string videoFilter = "System.Kind:=System.Kind#Video ext:<>.srt";
        internal readonly static List<string> VideoFileTypes = new List<string> { ".webm", ".flv", ".mkv", ".vob", ".ogv", ".ogg",
                                                         ".drc", ".gif", ".gifv", ".mng", ".avi",".mov",
                                                         ".qt",".wmv",".yuv",".rm",".rmvb",".asf",".amv",
                                                         ".mp4",".m4p",".m4v",".mpg",".mp2",".mpeg",".mpe",
                                                         ".mpv",".m2v",".svi",".3gp",".3g2",".mxf",
                                                         ".roq",".nsv",".f4v",".f4p",".f4a",".f4b",".divx"};



        internal readonly static string[] RequiredVideoProperties = new String[] { "System.Media.Duration" };
        private static QueryOptions videoFileOptions;
        private static QueryOptions videoFolderOptions;
        #endregion
        #region StaticMethods
        //internal static string AddToLikes(StorageFile videoToLike)
        //{
        //    string token = "";
        //    if (AreLikesFull())
        //        token = "";

        //    token = StorageApplicationPermissions.FutureAccessList.Add(videoToLike);
        //    return token;
        //}
        //internal static void RemoveFromLikes(string tokenToUnLike) => StorageApplicationPermissions.FutureAccessList.Remove(tokenToUnLike);
        internal async static Task<string> GetDuration(StorageFile file)
        {
            IDictionary<string, object> size = await file.Properties.RetrievePropertiesAsync(
                                                       FileHelper.RequiredVideoProperties);
            var ticks = Convert.ToInt64(size["System.Media.Duration"]);
            return TimeSpan.FromTicks(ticks).ToString(@"hh\:mm\:ss");
        }

        internal static void AddToHistory(StorageFile fileToBeAdded)
        {
            if (StorageApplicationPermissions.FutureAccessList.Entries.Count >= 999)
            {
                string tokenToRemove = StorageApplicationPermissions.FutureAccessList.Entries.Last().Token;
                StorageApplicationPermissions.FutureAccessList.Remove(tokenToRemove);
            }
            StorageApplicationPermissions.FutureAccessList.Add(fileToBeAdded);
        }
        //internal static bool AreLikesFull()
        //{
        //    bool likesAreFull = false;
        //    if (StorageApplicationPermissions.FutureAccessList.Entries.Count < 999)
        //        likesAreFull = false;
        //    else
        //        likesAreFull = true;

        //    return likesAreFull;
        //}


        internal static async Task<BitmapImage> GetDisplayForFile(StorageFile MyVideoFile)
        {
            var bitm = new BitmapImage();
            using (var imgSource = await MyVideoFile?.GetScaledImageAsThumbnailAsync(ThumbnailMode.VideosView, Constants._thumbnailReqestedSize, ThumbnailOptions.UseCurrentScale))
            {
                if (imgSource != null) { await bitm.SetSourceAsync(imgSource); return bitm; }
            }
            return null;
        }

        internal static StorageFileQueryResult GetVideoFilesQuery(StorageFolder Folder, uint thumbnailRequestedSize)
        {
            if (videoFileOptions is default(QueryOptions))
            {
                videoFileOptions = new QueryOptions()
                {
                    IndexerOption = IndexerOption.UseIndexerWhenAvailable
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

        internal static StorageFolderQueryResult GetVideoFoldersQuery(StorageFolder Folder)
        {
            if (videoFolderOptions is default(QueryOptions))
            {
                videoFolderOptions = new QueryOptions(CommonFolderQuery.DefaultQuery)
                {
                    IndexerOption = IndexerOption.UseIndexerWhenAvailable
                };
                //videoFolderOptions.SetPropertyPrefetch(PropertyPrefetchOptions.VideoProperties, RequiredVideoProperties);
            }
            return Folder.CreateFolderQueryWithOptions(videoFolderOptions);
        }

        public static async Task Rate()
        {
            //await Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", Windows.ApplicationModel.Package.Current.Id.FamilyName)));
            StoreSendRequestResult result = await StoreRequestHelper.SendRequestAsync(StoreContext.GetDefault(), 16, String.Empty);
            if (result.ExtendedError is null)
            {
                JObject jsonObject = JObject.Parse(result.Response);
                if (jsonObject.SelectToken("status").ToString() == "success")
                {
                    //return true because rating dialog was shown and app was rated
                    await ApplicationData.Current.RoamingSettings.SaveAsync("hasRated", true);
                }
            }
            //return false, there was an error or customer ignored the rating
        }
        #endregion

    }
}
