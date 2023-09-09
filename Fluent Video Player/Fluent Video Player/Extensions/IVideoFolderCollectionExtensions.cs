using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using Microsoft.AppCenter.Analytics;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;

namespace Fluent_Video_Player.Extensions
{
    public static class IVideoFolderCollectionExtensions
    {
        public static async Task FillHistoryAsync(this ObservableCollection<Video> videos, int? maxItems = null)
        {
            Analytics.TrackEvent("FillHistoryAsync()");
            if (videos.Count > 0)
            {
                videos.Clear();
            }
            List<string> ToRemoveTokens = new();
            foreach (var entry in StorageApplicationPermissions.FutureAccessList.Entries)
            {
                try
                {
                    var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(entry.Token);
                    Video vv = new(file);
                    videos.Add(vv);

                    await vv.SetHistoryTokenDurationAndThumbnailAsync(file, entry.Token);
                }
                catch (System.IO.FileNotFoundException)
                {
                    ToRemoveTokens.Add(entry.Token);
                }
                if (maxItems.HasValue && videos.Count >= maxItems)
                {
                    break;
                }
            }
            foreach (var item in ToRemoveTokens)
            {
                StorageApplicationPermissions.FutureAccessList.Remove(item);
            }
        }
    }
}
