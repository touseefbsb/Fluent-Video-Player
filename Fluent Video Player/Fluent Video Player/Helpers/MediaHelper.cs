using Fluent_Video_Player.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Search;
using Microsoft.AppCenter.Analytics;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Fluent_Video_Player.Helpers
{

    public class CurrentItemOpenedEventArgs : EventArgs
    {
        public string CurrentTitle { get; set; } = "";
        public TimeSpan ResumePosition { get; set; } = TimeSpan.Zero;
    }
    public class CurrentPlaybackListItemChangedEventArgs : EventArgs
    {
        public bool PreviousButtonEnabled { get; set; }
        public bool NextButtonEnabled { get; set; }
    }
    public class MainMediaPlayer
    {
        public MediaPlayer MP => CurrentPlaybackList.MP;

        public string PlaylistTitle { get; private set; } = "";

        public readonly FluentPlaybackList CurrentPlaybackList;
        public void Play() => MP.Play();
        public void Pause() => MP.Pause();
        public event EventHandler<CurrentPlaybackListItemChangedEventArgs> CurrentPlaybackListItemChanged;
        public event EventHandler<CurrentItemOpenedEventArgs> CurrentItemOpened;
        public event EventHandler<EventArgs> InitialItemOpened;

        public MainMediaPlayer()
        {
            CurrentPlaybackList = new FluentPlaybackList();
            CurrentPlaybackList.InitialOpened += CurrentPlaybackList_InitialOpened;
            MP.MediaOpened += MP_MediaOpened;
            CurrentPlaybackList.CurrentItemChanged += CurrentPlaybackList_CurrentItemChanged;
        }

        private void CurrentPlaybackList_InitialOpened(object sender, EventArgs e) => InitialItemOpened?.Invoke(this, EventArgs.Empty);

        private async void MP_MediaOpened(MediaPlayer sender, object args)
        {
            var props = CurrentPlaybackList?.CurrentItem?.GetDisplayProperties()?.VideoProperties;
            var openArgs = new CurrentItemOpenedEventArgs();
            if (!(props is null))
            {
                openArgs.CurrentTitle = props.Title;
                openArgs.ResumePosition = await Database.DbHelper.GetPosition(props.Subtitle);
            }
            CurrentItemOpened?.Invoke(this, openArgs);
        }

        private void CurrentPlaybackList_CurrentItemChanged(object sender, CurrentItemChangedEventArgs args)
        {
            var changeArgs = new CurrentPlaybackListItemChangedEventArgs();
            switch (args.Reason)
            {
                case MediaPlaybackItemChangedReason.InitialItem:
                    break;
                case MediaPlaybackItemChangedReason.EndOfStream:
                    break;
                case MediaPlaybackItemChangedReason.Error:
                    break;
                case MediaPlaybackItemChangedReason.AppRequested:
                    break;
                default:
                    break;
            }
            Play();//play anyway whatever the reason is
            var looping = CheckLooping();
            changeArgs.PreviousButtonEnabled = looping.Item1;
            changeArgs.NextButtonEnabled = looping.Item2;
            CurrentPlaybackListItemChanged?.Invoke(this, changeArgs);
        }

        public Tuple<bool, bool> CheckLooping()
        {
            bool previousButtonEnabled = false;
            bool nextButtonEnabled = false;
            if (CurrentPlaybackList != default(object))
            {
                if (CurrentPlaybackList.AutoRepeatEnabled)
                {
                    previousButtonEnabled = true;
                    nextButtonEnabled = true;
                }
                else
                {
                    if (CurrentPlaybackList.GetCurrentListMode() != CurrentListMode.None)
                    {
                        var currentIndex = CurrentPlaybackList.CurrentItemIndex;
                        if (CurrentPlaybackList.GetCurrentListMode() == CurrentListMode.Single)
                        {
                            previousButtonEnabled = false;
                            nextButtonEnabled = false;
                        }
                        else if (currentIndex < CurrentPlaybackList.ItemsLastIndex &&
                            currentIndex > 0)
                        {
                            previousButtonEnabled = true;
                            nextButtonEnabled = true;
                        }
                        else if (currentIndex == 0)
                        {
                            previousButtonEnabled = false;
                            nextButtonEnabled = true;
                        }
                        else if (currentIndex == CurrentPlaybackList.ItemsLastIndex)
                        {
                            previousButtonEnabled = true;
                            nextButtonEnabled = false;
                        }
                    }
                }
            }
            return new Tuple<bool, bool>(previousButtonEnabled, nextButtonEnabled);
        }
        internal async Task SetNewSourceWithVideoCollection<T>(Video clickedItem, ObservableCollection<T> sourcePrivate, string playlistTitle = "")
    where T : IVideoFolder // Constraint ensures T implements IVideoFolder
        {
            PlaylistTitle = playlistTitle;
            if (clickedItem is not null)
            {
                int startingIndex = 0;
                CurrentPlaybackList.Items.Clear();
                var videos = new List<Video>();

                // Cast items to Video type if they are actually Videos
                foreach (var item in sourcePrivate)
                {
                    if (item is Video video)
                        videos.Add(video);
                }

                if (videos.Count < 2)
                {
                    CurrentPlaybackList.Items.Add(new FluentPlaybackItem(clickedItem));
                }
                else
                {
                    for (int i = 0; i < videos.Count; i++)
                    {
                        CurrentPlaybackList.Items.Add(new FluentPlaybackItem(videos[i]));
                        if (clickedItem == videos[i])
                        {
                            startingIndex = i;
                        }
                    }
                }

                await CurrentPlaybackList.SetSource(startingIndex);
            }
        }

        /// <summary>
        /// Will be used when any video played from within the app excluding open button on controls.
        /// </summary>
        /// <param name="newFile">incoming file to be set as source</param>
        public async Task SetNewSource(StorageFile newFile)
        {
            PlaylistTitle = "";
            CurrentPlaybackList.Items.Clear();
            var vv = new Video(newFile);
            CurrentPlaybackList.Items.Add(new FluentPlaybackItem(vv));
            await CurrentPlaybackList.SetSource();

            vv.Duration = await FileHelper.GetDuration(newFile);
            BitmapImage bitm = await FileHelper.GetDisplayForFile(vv.MyVideoFile);
            if (!(bitm is null))
                vv.Thumbnail = bitm;
        }


        /// <summary>
        /// Will be used when fileactivated through the app. along with neighbouring items, or file opened with picker
        /// from within the app
        /// </summary>
        /// <param name="files">files which will be coming from fileactivation</param>
        /// <param name="neighboringFilesQuery"> if any get the neighbour files</param>
        internal async Task SetNewSourceWithFiles(IReadOnlyList<IStorageItem> clickedFiles, StorageFileQueryResult neighboringFilesQuery = null)
        {
            if (clickedFiles.Count > 0)
            {
                PlaylistTitle = "";
                int startingIndex = 0;
                CurrentPlaybackList.Items.Clear();
                if (clickedFiles.Count == 1)
                {
                    var file = (StorageFile)clickedFiles.FirstOrDefault();
                    var vv = new Video(file);
                    if (!(neighboringFilesQuery is null))
                    {
                        var neighbourFiles = await neighboringFilesQuery.GetFilesAsync();
                        var neighbourVideoFiles = new List<StorageFile>();
                        foreach (var neighbourFile in neighbourFiles)
                        {
                            if (FileHelper.VideoFileTypes.Contains(neighbourFile.FileType.ToLower()))
                            {
                                neighbourVideoFiles.Add(neighbourFile);
                            }
                        }
                        if (neighbourVideoFiles.Count > 1)
                        {
                            PlaylistTitle = neighbourVideoFiles[0]?.Provider?.DisplayName ?? "";
                            try
                            {
                                startingIndex = neighbourVideoFiles.FindIndex(x => x.Path == file.Path);
                            }
                            catch
                            {
                                Analytics.TrackEvent("OverFlowException, filetype : " + file.FileType);
                                CurrentPlaybackList.Items.Add(new FluentPlaybackItem(vv));
                                vv.Duration = await FileHelper.GetDuration(vv.MyVideoFile);
                                BitmapImage bitm = await FileHelper.GetDisplayForFile(vv.MyVideoFile);
                                if (!(bitm is null))
                                    vv.Thumbnail = bitm;
                            }
                            foreach (var f in neighbourVideoFiles)
                            {
                                var vvf = new Video(f);
                                CurrentPlaybackList.Items.Add(new FluentPlaybackItem(vvf));
                                vvf.Duration = await FileHelper.GetDuration(vvf.MyVideoFile);
                                BitmapImage bitm = await FileHelper.GetDisplayForFile(vvf.MyVideoFile);
                                if (!(bitm is null))
                                    vvf.Thumbnail = bitm;
                            }
                        }
                        else
                        {
                            CurrentPlaybackList.Items.Add(new FluentPlaybackItem(vv));
                            vv.Duration = await FileHelper.GetDuration(vv.MyVideoFile);
                            BitmapImage bitm = await FileHelper.GetDisplayForFile(vv.MyVideoFile);
                            if (!(bitm is null))
                                vv.Thumbnail = bitm;
                        }
                    }
                    else
                    {
                        CurrentPlaybackList.Items.Add(new FluentPlaybackItem(vv));
                        vv.Duration = await FileHelper.GetDuration(vv.MyVideoFile);
                        BitmapImage bitm = await FileHelper.GetDisplayForFile(vv.MyVideoFile);
                        if (!(bitm is null))
                            vv.Thumbnail = bitm;
                    }
                }
                else
                {
                    var parentName = ((StorageFile)clickedFiles[0])?.Provider?.DisplayName;
                    PlaylistTitle = parentName ?? "";
                    for (int i = 0; i < clickedFiles.Count; i++)
                    {
                        var file = (StorageFile)clickedFiles[i];
                        var vv = new Video(file);
                        CurrentPlaybackList.Items.Add(new FluentPlaybackItem(vv));
                        vv.Duration = await FileHelper.GetDuration(vv.MyVideoFile);
                        BitmapImage bitm = await FileHelper.GetDisplayForFile(vv.MyVideoFile);
                        if (!(bitm is null))
                            vv.Thumbnail = bitm;
                    }
                }
                await CurrentPlaybackList.SetSource(startingIndex);
            }
        }
        //public bool CurrentVideoIsLiked => CurrentVideo.Liked;

        /// <summary>
        /// tries to like the video and store it in futureacceslist
        /// </summary>
        /// <returns>returns the success in boolean</returns>
        //public async Task<bool> LikeCurrentVideo()
        //{
        //    string token = FileHelper.AddToLikes(CurrentVideo.MyVideoFile);
        //    if (string.IsNullOrWhiteSpace(token))
        //        return false;

        //    CurrentVideo.LikedToken = token;
        //    CurrentVideo.Liked = true;
        //    string subtitle = "fvpliked" + token;
        //    CurrentVideo.Subtitle = subtitle;// subtitle is used for folder relativeid by default
        //    List<KeyValuePair<string, object>> props = new List<KeyValuePair<string, object>>
        //    {
        //        new KeyValuePair<string, object>("System.Media.Subtitle",subtitle)
        //    };
        //    await CurrentVideo.MyVideoFile.Properties.SavePropertiesAsync(props.AsEnumerable());
        //    return CurrentVideo.Liked;
        //}

        //public async Task UnlikeCurrentVideo()
        //{
        //    FileHelper.RemoveFromLikes(CurrentVideo.LikedToken);
        //    CurrentVideo.LikedToken = "";
        //    CurrentVideo.Liked = false;
        //    string subtitle = "";
        //    CurrentVideo.Subtitle = subtitle;
        //    List<KeyValuePair<string, object>> props = new List<KeyValuePair<string, object>>
        //    {
        //        new KeyValuePair<string, object>("System.Media.Subtitle",subtitle)
        //    };
        //    await CurrentVideo.MyVideoFile.Properties.SavePropertiesAsync(props.AsEnumerable());
        //}

        /// <summary>
        /// This method toggles the volume with keyboard shortcut up and down on the desktop.
        /// </summary>
        /// <param name="up">if up is true volume will go up else down.</param>
        public void ToggleVolume(bool up)
        {
            if (up)
            {
                if (MP.Volume != 1.0)
                {
                    if (MP.Volume > 0.95)
                        MP.Volume = 1.0;
                    else
                        MP.Volume += 0.05;
                }
            }
            else
            {
                if (MP.Volume != 0.0)
                {
                    if (MP.Volume <= 0.07)
                        MP.Volume = 0.0;
                    else
                        MP.Volume -= 0.05;
                }
            }
        }

        //public async Task<bool> CurrentLikeEnabled()
        //{
        //    if (!FileHelper.AreLikesFull())
        //    {
        //        if (true)//check if the current file is liked.//hint : use ApplicationLocalData from settings
        //            return true;
        //    }
        //    else
        //        return false;
        //}

        public void TogglePosition(bool v)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(5);
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
                timeSpan = TimeSpan.FromSeconds(30);
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))            
                timeSpan = TimeSpan.FromSeconds(60);

            if (v)
                MP.PlaybackSession.Position += timeSpan;
            else
                MP.PlaybackSession.Position -= timeSpan;
        }

        public bool ToggleMute()
        {
            MP.IsMuted = !MP.IsMuted;
            return MP.IsMuted;
        }

        public void TogglePlayPause()
        {
            if (MP.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                Pause();
            else
                Play();
        }

        public void VolumeChanged(double newValue)
        {
            MP.Volume = newValue / 100.0;
            //MP.IsMuted = newValue == 0;dont mute if volume is 0
        }

    }
    public static class MediaHelper
    {
        public static MainMediaPlayer MyMediaPlayer { get; } = new MainMediaPlayer();
    }
}
