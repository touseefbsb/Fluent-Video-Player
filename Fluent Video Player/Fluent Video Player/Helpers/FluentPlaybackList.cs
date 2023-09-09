using FFmpegInteropX;
using Fluent_Video_Player.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Fluent_Video_Player.Helpers
{
    public class CurrentItemChangedEventArgs : EventArgs
    {
        public MediaPlaybackItemChangedReason Reason { get; set; }
    }
    public class FluentPlaybackList
    {
        private FFmpegMediaSource FFmpegMSS;
        private MediaSourceConfig Config { get; }
        public MediaPlayer MP { get; }
        public ObservableCollection<FluentPlaybackItem> Items { get; }
        public FluentPlaybackItem CurrentItem { get; private set; }
        public int CurrentItemIndex => Items.IndexOf(CurrentItem);
        public bool AutoRepeatEnabled { get; set; }
        public event EventHandler<CurrentItemChangedEventArgs> CurrentItemChanged;
        public event EventHandler<EventArgs> InitialOpened;

        public CurrentListMode GetCurrentListMode()
        {
            if (Items.Count == 0)
                return CurrentListMode.None;
            else if (Items.Count == 1)
                return CurrentListMode.Single;
            else
                return CurrentListMode.Multiple;
        }
        public FluentPlaybackList()
        {
            Items = new ObservableCollection<FluentPlaybackItem>();
            MP = new MediaPlayer()
            {
                AudioCategory = MediaPlayerAudioCategory.Movie// so that it takes priority over other media
            };
            Config = new MediaSourceConfig();
            MP.MediaEnded += MP_MediaEnded;
            MP.MediaFailed += MP_MediaFailed;
            MP.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            MP.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;

            MP.SystemMediaTransportControls.ButtonPressed += SystemMediaTransportControls_ButtonPressed;
        }

        private async void CommandManager_NextReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerNextReceivedEventArgs args)
        {
            await MoveNext(MediaPlaybackItemChangedReason.AppRequested);
        }

        private async void SystemMediaTransportControls_ButtonPressed(Windows.Media.SystemMediaTransportControls sender, Windows.Media.SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            if (args.Button == Windows.Media.SystemMediaTransportControlsButton.Next)
            {
                await MoveNext(MediaPlaybackItemChangedReason.AppRequested);
            }
            else if (args.Button == Windows.Media.SystemMediaTransportControlsButton.Previous)
            {
                await MovePrevious(MediaPlaybackItemChangedReason.AppRequested);
            }
        }

        private async void MP_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args) =>
            await MoveNext(MediaPlaybackItemChangedReason.Error);

        private async void MP_MediaEnded(MediaPlayer sender, object args) => await MoveNext(MediaPlaybackItemChangedReason.EndOfStream);

        private async Task SetCurrentItem(FluentPlaybackItem newCurrentItem, MediaPlaybackItemChangedReason reason)
        {
            CurrentItem = newCurrentItem;
            //assign new item only if it is null
            if (CurrentItem.Item is null)
                CurrentItem.Item = await GetMediaPlayBackItem(CurrentItem.VideoFile);

            MP.Source = CurrentItem.Item;
            OnCurrentItemChanged(reason);
        }

        private async void OnCurrentItemChanged(MediaPlaybackItemChangedReason reason)
        {
            //keep open items stuff
            await KeepOpenItems();
            CurrentItemChanged?.Invoke(this, new CurrentItemChangedEventArgs { Reason = reason });
        }

        private async Task KeepOpenItems()
        {
            await Task.Run(() =>
            {
                if (Items.Count > 3)// means items are atleast 4
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (i != CurrentItemIndex && i != CurrentItemIndex - 1 && i != CurrentItemIndex + 1 && Items[i].Item != default(MediaPlaybackItem))
                        {
                            Items[i].Item.Source.Dispose();
                            Items[i].Item = default(MediaPlaybackItem);
                        }
                    }
                }
            });
        }

        public async Task SetSource(int startingIndex = 0)
        {
            var reason = MediaPlaybackItemChangedReason.InitialItem;
            if (GetCurrentListMode() == CurrentListMode.Single)
            {
                await SetCurrentItem(Items[0], reason);
            }
            else if (GetCurrentListMode() == CurrentListMode.Multiple)
            {
                await SetCurrentItem(Items[startingIndex], reason);
            }
            InitialOpened?.Invoke(this, EventArgs.Empty);
        }

        private async Task<MediaPlaybackItem> GetMediaPlayBackItem(StorageFile myVideoFile)
        {

            IRandomAccessStream readStream = await myVideoFile.OpenAsync(FileAccessMode.Read);
            FFmpegMSS = await FFmpegMediaSource.CreateFromStreamAsync(readStream, Config);
            var mediaPlaybackItem = FFmpegMSS.CreateMediaPlaybackItem();
            //var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(myVideoFile));
            MediaItemDisplayProperties props = mediaPlaybackItem?.GetDisplayProperties();
            if (!(props is null))
            {
                props.Type = Windows.Media.MediaPlaybackType.Video;
                props.VideoProperties.Title = myVideoFile.DisplayName;
                props.VideoProperties.Subtitle = myVideoFile.Path;
                //var thumb = await myVideoFile?.GetThumbnailAsync(ThumbnailMode.VideosView, 190, ThumbnailOptions.UseCurrentScale);
                //if (thumb != default(object))
                //{
                //    props.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumb);
                //}
                mediaPlaybackItem.ApplyDisplayProperties(props);
            }
            FileHelper.AddToHistory(myVideoFile);
            return mediaPlaybackItem;

        }

        public async Task MoveNext(MediaPlaybackItemChangedReason reason)
        {
            if (GetCurrentListMode() != CurrentListMode.None)
            {
                if (AutoRepeatEnabled)
                {
                    if (GetCurrentListMode() == CurrentListMode.Single)
                    {
                        await SetCurrentItem(Items[0], reason);
                    }
                    else if (GetCurrentListMode() == CurrentListMode.Multiple)
                    {
                        if (CurrentItemIndex < ItemsLastIndex)
                            await SetCurrentItem(Items[CurrentItemIndex + 1], reason);
                        else
                            await SetCurrentItem(Items[0], reason);//move to first item ( looping )
                    }
                }
                else if (GetCurrentListMode() == CurrentListMode.Multiple && CurrentItemIndex < ItemsLastIndex)
                {
                    await SetCurrentItem(Items[CurrentItemIndex + 1], reason);
                }
            }
        }

        public int ItemsLastIndex => Items.Count - 1;

        public async Task MovePrevious(MediaPlaybackItemChangedReason reason)
        {
            if (GetCurrentListMode() != CurrentListMode.None)
            {
                if (AutoRepeatEnabled)
                {
                    if (GetCurrentListMode() == CurrentListMode.Single)
                    {
                        await SetCurrentItem(Items[0], reason);
                    }
                    else if (GetCurrentListMode() == CurrentListMode.Multiple)
                    {
                        if (CurrentItemIndex > 0)
                            await SetCurrentItem(Items[CurrentItemIndex - 1], reason);
                        else
                            await SetCurrentItem(Items[ItemsLastIndex], reason);//move to last item ( looping )
                    }
                }
                else if (GetCurrentListMode() == CurrentListMode.Multiple && CurrentItemIndex > 0)
                {
                    await SetCurrentItem(Items[CurrentItemIndex - 1], reason);
                }
            }
        }

        public async Task MoveToItem(FluentPlaybackItem item, MediaPlaybackItemChangedReason reason = MediaPlaybackItemChangedReason.AppRequested)
        {
            if (Items.Contains(item) && CurrentItem != item)
            {
                await SetCurrentItem(item, reason);
            }
        }
    }
    public class FluentPlaybackItem
    {
        public FluentPlaybackItem(Video myVideo) => MyVideo = myVideo;
        public Video MyVideo { get; set; }
        public MediaPlaybackItem Item { get; set; }
        public StorageFile VideoFile => MyVideo?.MyVideoFile;
        public MediaItemDisplayProperties GetDisplayProperties() => Item?.GetDisplayProperties();
        public FluentPlaybackItem THIS => this;
    }
    public enum CurrentListMode
    {
        None,
        Single,
        Multiple
    }
}
