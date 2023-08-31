using System.Collections.ObjectModel;
using Fluent_Video_Player.Core.Enums;
using Fluent_Video_Player.EventArgs;
using Fluent_Video_Player.Models;
using Windows.Media.Playback;
using Windows.Storage;
using FFmpegInteropX;
using Fluent_Video_Player.Contracts.Services;

namespace Fluent_Video_Player.Helpers
{
    public class FluentPlaybackList
    {
        #region Services
        private readonly IStorageFileService _storageFileService = App.GetService<IStorageFileService>();
        #endregion Services

        #region Props
        internal FFmpegMediaSource FFmpegMSS { get; private set; }
        public MediaPlayer MP { get; }
        public ObservableCollection<FluentPlaybackItem> Items { get; }
        public FluentPlaybackItem CurrentItem { get; private set; }
        public int CurrentItemIndex => Items.IndexOf(CurrentItem);
        public bool AutoRepeatEnabled { get; set; }
        public int ItemsLastIndex => Items.Count - 1;
        #endregion Props

        #region Events
        public event EventHandler<CurrentItemChangedEventArgs> CurrentItemChanged;
        public event EventHandler<System.EventArgs> InitialOpened;
        #endregion Events

        #region Ctor
        public FluentPlaybackList()
        {
            Items = new ObservableCollection<FluentPlaybackItem>();
            MP = new MediaPlayer()
            {
                AudioCategory = MediaPlayerAudioCategory.Movie, // so that it takes priority over other media
            };
            MP.MediaEnded += MP_MediaEnded;
            MP.MediaFailed += MP_MediaFailed;
            MP.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            MP.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            MP.SystemMediaTransportControls.ButtonPressed += SystemMediaTransportControls_ButtonPressed;
        }
        #endregion Ctor

        #region Methods        
        public CurrentListMode GetCurrentListMode() => Items.Count switch
        {
            0 => CurrentListMode.None,
            1 => CurrentListMode.Single,
            _ => CurrentListMode.Multiple
        };
        private async void CommandManager_NextReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerNextReceivedEventArgs args) =>
            await MoveNextAsync(MediaPlaybackItemChangedReason.AppRequested);

        private async void SystemMediaTransportControls_ButtonPressed(Windows.Media.SystemMediaTransportControls sender, Windows.Media.SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            if (args.Button == Windows.Media.SystemMediaTransportControlsButton.Next)
            {
                await MoveNextAsync(MediaPlaybackItemChangedReason.AppRequested);
            }
            else if (args.Button == Windows.Media.SystemMediaTransportControlsButton.Previous)
            {
                await MovePreviousAsync(MediaPlaybackItemChangedReason.AppRequested);
            }
        }

        private async void MP_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args) =>
            await MoveNextAsync(MediaPlaybackItemChangedReason.Error);

        private async void MP_MediaEnded(MediaPlayer sender, object args) => await MoveNextAsync(MediaPlaybackItemChangedReason.EndOfStream);

        private async Task SetCurrentItemAsync(FluentPlaybackItem newCurrentItem, MediaPlaybackItemChangedReason reason)
        {
            CurrentItem = newCurrentItem;
            //assign new item only if it is null
            if (CurrentItem.Item is null)
            {
                CurrentItem.Item = await GetMediaPlayBackItemAsync(CurrentItem.VideoFile);
            }

            MP.Source = CurrentItem.Item;
            OnCurrentItemChanged(reason);
        }

        private async void OnCurrentItemChanged(MediaPlaybackItemChangedReason reason)
        {
            //keep open items stuff
            await KeepOpenItemsAsync();
            CurrentItemChanged?.Invoke(this, new CurrentItemChangedEventArgs { Reason = reason });
        }

        private async Task KeepOpenItemsAsync() => await Task.Run(() =>
        {
            if (Items.Count > 3) // means items are atleast 4
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    if (i != CurrentItemIndex && i != CurrentItemIndex - 1 && i != CurrentItemIndex + 1 && Items[i].Item != default)
                    {
                        Items[i].DisposeMediaPlaybackItem();
                    }
                }
            }
        });

        public async Task SetSourceAsync(int startingIndex = 0)
        {
            var reason = MediaPlaybackItemChangedReason.InitialItem;
            if (GetCurrentListMode() == CurrentListMode.Single)
            {
                await SetCurrentItemAsync(Items[0], reason);
            }
            else if (GetCurrentListMode() == CurrentListMode.Multiple)
            {
                await SetCurrentItemAsync(Items[startingIndex], reason);
            }
            InitialOpened?.Invoke(this, System.EventArgs.Empty);
        }

        private async Task<MediaPlaybackItem?> GetMediaPlayBackItemAsync(StorageFile? myVideoFile)
        {
            if (myVideoFile is null)
            {
                return null;
            }

            var readStream = await myVideoFile.OpenAsync(FileAccessMode.Read);
            FFmpegMSS = await FFmpegMediaSource.CreateFromStreamAsync(readStream, new MediaSourceConfig());
            var mediaPlaybackItem = FFmpegMSS.CreateMediaPlaybackItem();
            var props = mediaPlaybackItem.GetDisplayProperties();
            if (props is not null)
            {
                props.Type = Windows.Media.MediaPlaybackType.Video;
                props.VideoProperties.Title = myVideoFile.DisplayName;
                props.VideoProperties.Subtitle = myVideoFile.Path;
                mediaPlaybackItem.ApplyDisplayProperties(props);
            }
            _storageFileService.AddToHistory(myVideoFile);
            return mediaPlaybackItem;
        }

        public async Task MoveNextAsync(MediaPlaybackItemChangedReason reason)
        {
            if (GetCurrentListMode() != CurrentListMode.None)
            {
                if (AutoRepeatEnabled)
                {
                    if (GetCurrentListMode() == CurrentListMode.Single)
                    {
                        await SetCurrentItemAsync(Items[0], reason);
                    }
                    else if (GetCurrentListMode() == CurrentListMode.Multiple)
                    {
                        if (CurrentItemIndex < ItemsLastIndex)
                        {
                            await SetCurrentItemAsync(Items[CurrentItemIndex + 1], reason);
                        }
                        else
                        {
                            await SetCurrentItemAsync(Items[0], reason); //move to first item ( looping )
                        }
                    }
                }
                else if (GetCurrentListMode() == CurrentListMode.Multiple && CurrentItemIndex < ItemsLastIndex)
                {
                    await SetCurrentItemAsync(Items[CurrentItemIndex + 1], reason);
                }
            }
        }

        public async Task MovePreviousAsync(MediaPlaybackItemChangedReason reason)
        {
            if (GetCurrentListMode() != CurrentListMode.None)
            {
                if (AutoRepeatEnabled)
                {
                    if (GetCurrentListMode() == CurrentListMode.Single)
                    {
                        await SetCurrentItemAsync(Items[0], reason);
                    }
                    else if (GetCurrentListMode() == CurrentListMode.Multiple)
                    {
                        if (CurrentItemIndex > 0)
                        {
                            await SetCurrentItemAsync(Items[CurrentItemIndex - 1], reason);
                        }
                        else
                        {
                            await SetCurrentItemAsync(Items[ItemsLastIndex], reason);//move to last item ( looping )
                        }
                    }
                }
                else if (GetCurrentListMode() == CurrentListMode.Multiple && CurrentItemIndex > 0)
                {
                    await SetCurrentItemAsync(Items[CurrentItemIndex - 1], reason);
                }
            }
        }

        public async Task MoveToItemAsync(FluentPlaybackItem item, MediaPlaybackItemChangedReason reason = MediaPlaybackItemChangedReason.AppRequested)
        {
            if (Items.Contains(item) && CurrentItem != item)
            {
                await SetCurrentItemAsync(item, reason);
            }
        }
        #endregion Methods
    }
}
