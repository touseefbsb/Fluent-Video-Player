using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using System;
using Windows.Storage;

namespace Fluent_Video_Player.Extensions
{
    public static class IVideoFolderExtensions
    {
        private static void TryUpdateThumbnail(this IVideoFolder videoFolder, BitmapImage bitmapImage)
        {
            if (bitmapImage is not null)
            {
                videoFolder.Thumbnail = bitmapImage;
            }
        }
        private static async Task TryUpdateThumbnailAsync(this Video video)
        {
            BitmapImage bitmapImage = null;
            if (video.MyVideoFile != null)
            {
                using (var imgSource = await video.MyVideoFile.GetScaledImageAsThumbnailAsync(ThumbnailMode.VideosView, Constants._thumbnailReqestedSize, ThumbnailOptions.UseCurrentScale))
                {
                    if (imgSource is not null)
                    {
                        bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(imgSource);
                    }
                }
            }
            video.TryUpdateThumbnail(bitmapImage);
        }
        public static async Task SetDurationAndThumbnailAsync(this Video video, StorageFile file)
        {
            video.Duration = await FileHelper.GetDuration(file);
            await video.TryUpdateThumbnailAsync();
        }
        public static Task SetHistoryTokenDurationAndThumbnailAsync(this Video video, StorageFile file, string historyToken)
        {
            video.HistoryToken = historyToken;
            return video.SetDurationAndThumbnailAsync(file);
        }
    }
}
