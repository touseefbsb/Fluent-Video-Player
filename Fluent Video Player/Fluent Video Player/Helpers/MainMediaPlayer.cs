using System.Collections.ObjectModel;
using Database;
using Fluent_Video_Player.Contracts.Services;
using Fluent_Video_Player.Core.Enums;
using Fluent_Video_Player.Enums;
using Fluent_Video_Player.EventArgs;
using Fluent_Video_Player.Models;
using Microsoft.AppCenter.Analytics;
using Microsoft.UI.Xaml;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System;
using Windows.UI.Core;

namespace Fluent_Video_Player.Helpers
{
    public class MainMediaPlayer
    {
        #region Services
        private readonly IStorageFileService _storageFileService = App.GetService<IStorageFileService>();
        private readonly IDbService _dbService = App.GetService<IDbService>();
        #endregion Services

        #region Fields

        #region KeyboardShortCuts

        #region ToggleVolume
        private const double MaxVolume = 1.0;
        private const double MinVolume = 0.0;
        private const double VolumeIncrement = 0.05;
        private const double UpperVolumeThreshold = 0.95;
        private const double LowerVolumeThreshold = 0.07;
        #endregion ToggleVolume

        #region TogglePosition
        private const int SmallStepSeconds = 5;
        private const int MediumStepSeconds = 30;
        private const int LargeStepSeconds = 60;
        #endregion TogglePosition

        #endregion KeyboardShortCuts

        #endregion Fields

        #region Props
        internal int StartingIndex { get; private set; }
        internal string PlaylistTitle { get; private set; } = "";
        public MediaPlayer MP => CurrentPlaybackList.MP;
        public readonly FluentPlaybackList CurrentPlaybackList;
        #endregion Props

        #region Ctor
        public MainMediaPlayer()
        {
            CurrentPlaybackList = new FluentPlaybackList();
            CurrentPlaybackList.InitialOpened += CurrentPlaybackList_InitialOpened;
            MP.MediaOpened += MP_MediaOpened;
            CurrentPlaybackList.CurrentItemChanged += CurrentPlaybackList_CurrentItemChanged;
        }
        #endregion Ctor

        #region Events
        public event EventHandler<CurrentPlaybackListItemChangedEventArgs> CurrentPlaybackListItemChanged;
        public event EventHandler<CurrentItemOpenedEventArgs> CurrentItemOpened;
        public event EventHandler<System.EventArgs> InitialItemOpened;
        #endregion Events

        #region Methods
        public void Play() => MP.Play();
        public void Pause() => MP.Pause();
        private void CurrentPlaybackList_InitialOpened(object sender, System.EventArgs e) => InitialItemOpened?.Invoke(this, System.EventArgs.Empty);
        private async void MP_MediaOpened(MediaPlayer sender, object args)
        {
            var props = CurrentPlaybackList?.CurrentItem?.GetDisplayProperties()?.VideoProperties;
            var openArgs = new CurrentItemOpenedEventArgs();
            if (props is not null)
            {
                openArgs.CurrentTitle = props.Title;
                openArgs.ResumePosition = await _dbService.GetPositionAsync(props.Subtitle);
            }
            CurrentItemOpened?.Invoke(this, openArgs);
        }
        public void VolumeChanged(double newValue) => MP.Volume = newValue / 100.0;
        private void CurrentPlaybackList_CurrentItemChanged(object sender, CurrentItemChangedEventArgs args)
        {
            Play(); //play anyway whatever the reason is
            var looping = CheckLooping();
            var changeArgs = new CurrentPlaybackListItemChangedEventArgs
            {
                PreviousButtonEnabled = looping.PreviousButtonEnabled,
                NextButtonEnabled = looping.NextButtonEnabled,
            };
            CurrentPlaybackListItemChanged?.Invoke(this, changeArgs);
        }

        public (bool PreviousButtonEnabled, bool NextButtonEnabled) CheckLooping()
        {
            // If CurrentPlaybackList is null, return false for both
            if (CurrentPlaybackList == default)
            {
                return (false, false);
            }

            // If AutoRepeat is enabled, both buttons should be enabled
            if (CurrentPlaybackList.AutoRepeatEnabled)
            {
                return (true, true);
            }

            var currentListMode = CurrentPlaybackList.GetCurrentListMode();
            var currentIndex = CurrentPlaybackList.CurrentItemIndex;
            var itemsLastIndex = CurrentPlaybackList.ItemsLastIndex;

            // If list mode is None or Single, both buttons should be disabled
            if (currentListMode == CurrentListMode.None || currentListMode == CurrentListMode.Single)
            {
                return (false, false);
            }

            if (currentIndex > 0 && currentIndex < itemsLastIndex)
            {
                return (true, true);
            }

            if (currentIndex == 0)
            {
                return (false, true);
            }

            if (currentIndex == itemsLastIndex)
            {
                return (true, false);
            }

            // Fallback, should ideally never be reached
            return (false, false);
        }
        #endregion Methods

        #region SetNewSource_Methods
        /// <summary>
        /// Will be used when any video played from within the app excluding open button on controls.
        /// </summary>
        /// <param name="newFile">incoming file to be set as source</param>
        public async Task SetNewSourceAsync(StorageFile newFile)
        {
            InitializePlaylist();
            await AddFileToPlaylistAsync(newFile);
            await CurrentPlaybackList.SetSourceAsync();
        }

        /// <summary>
        /// When multiple videos are played from within the app e.g : playlist, folder
        /// from within the app
        /// </summary>
        /// <param name="clickedItem">clicked video from app</param>
        /// <param name="sourcePrivate">its sibling videos like from a folder or a playlist</param>
        internal async Task SetNewSourceAsync(Video clickedItem, ObservableCollection<IVideoFolder> sourcePrivate, string playlistTitle = "")
        {
            if (clickedItem is null)
            {
                return;
            }

            InitializePlaylist(playlistTitle);

            var videos = sourcePrivate.OfType<Video>().ToList();

            if (videos.Count < 2)
            {
                AddVideoToPlaylist(clickedItem);
            }
            else
            {
                AddVideosToPlaylist(videos, clickedItem);
            }
            await CurrentPlaybackList.SetSourceAsync(StartingIndex);
        }

        /// <summary>
        /// Will be used when fileactivated through the app. along with neighbouring items, or file opened with picker
        /// from within the app
        /// </summary>
        /// <param name="files">files which will be coming from fileactivation</param>
        /// <param name="neighboringFilesQuery"> if any get the neighbour files</param>
        public async Task SetNewSourceAsync(IReadOnlyList<IStorageItem> files, StorageFileQueryResult? neighboringFilesQuery = null)
        {
            if (!files.Any())
            {
                return;
            }

            // Initialize or clear playlist
            InitializePlaylist();

            if (files.Count == 1)
            {
                await HandleSingleFileAsync(files[0] as StorageFile, neighboringFilesQuery);
            }
            else
            {
                await HandleMultipleFilesAsync(files);
            }

            // Finalize playlist setup
            await FinalizePlaylistAsync();
        }
        #endregion SetNewSource_Methods

        #region SetNewSource_Helper_Methods
        private void InitializePlaylist(string playlistTitle = "")
        {
            StartingIndex = 0;
            PlaylistTitle = playlistTitle;
            CurrentPlaybackList.Items.Clear();
        }
        private async Task FinalizePlaylistAsync() => await CurrentPlaybackList.SetSourceAsync(StartingIndex);
        private async Task HandleSingleFileAsync(StorageFile? singleFile, StorageFileQueryResult? neighboringFilesQuery)
        {
            if (singleFile is null)
            {
                return;
            }

            if (neighboringFilesQuery is not null)
            {
                var neighbourVideoFiles = await GetNeighbourVideoFilesAsync(neighboringFilesQuery);

                if (neighbourVideoFiles.Count > 1)
                {
                    PlaylistTitle = neighbourVideoFiles.FirstOrDefault()?.Provider?.DisplayName ?? "";

                    try
                    {
                        StartingIndex = neighbourVideoFiles.FindIndex(x => x.Path == singleFile.Path);
                    }
                    catch (OverflowException)
                    {
                        Analytics.TrackEvent($"{nameof(HandleSingleFileAsync)} : OverFlowException, filetype : {singleFile.FileType}");
                        await AddFileToPlaylistAsync(singleFile);
                    }

                    foreach (var file in neighbourVideoFiles)
                    {
                        await AddFileToPlaylistAsync(file);
                    }
                }
                else
                {
                    await AddFileToPlaylistAsync(singleFile);
                }
            }
            else
            {
                await AddFileToPlaylistAsync(singleFile);
            }
        }
        private async Task HandleMultipleFilesAsync(IReadOnlyList<IStorageItem> clickedFiles)
        {
            PlaylistTitle = ((StorageFile)clickedFiles[0])?.Provider?.DisplayName ?? "";
            foreach (var file in clickedFiles.OfType<StorageFile>())
            {
                await AddFileToPlaylistAsync(file);
            }
        }
        private async Task<List<StorageFile>> GetNeighbourVideoFilesAsync(StorageFileQueryResult neighboringFilesQuery)
        {
            var neighbourFiles = await neighboringFilesQuery.GetFilesAsync();
            return neighbourFiles.Where(f => _storageFileService.VideoFileTypes.Contains(f.FileType, StringComparer.OrdinalIgnoreCase)).ToList();
        }
        private async Task AddFileToPlaylistAsync(StorageFile file)
        {
            var video = new Video(file);
            AddVideoToPlaylist(video);

            video.Duration = await _storageFileService.GetDurationAsync(video.MyVideoFile);
            var thumbnail = await _storageFileService.GetDisplayForFileAsync(video.MyVideoFile);
            if (thumbnail is not null)
            {
                video.Thumbnail = thumbnail;
            }
        }
        private void AddVideoToPlaylist(Video video) => CurrentPlaybackList.Items.Add(new FluentPlaybackItem(video));
        private void AddVideosToPlaylist(List<Video> videos, Video clickedItem)
        {
            for (var i = 0; i < videos.Count; i++)
            {
                AddVideoToPlaylist(videos[i]);
                if (clickedItem == videos[i])
                {
                    StartingIndex = i;
                }
            }
        }
        #endregion SetNewSource_Helper_Methods

        #region KeyboardShortcuts_Methods
        /// <summary>
        /// This method toggles the volume with keyboard shortcut up and down on the desktop.
        /// </summary>
        /// <param name="increase">if increase is true volume will go up else down.</param>
        public void ToggleVolume(bool increase)
        {
            if (increase)
            {
                MP.Volume = MP.Volume >= UpperVolumeThreshold ? MaxVolume : Math.Min(MP.Volume + VolumeIncrement, MaxVolume);
            }
            else
            {
                MP.Volume = MP.Volume <= LowerVolumeThreshold ? MinVolume : Math.Max(MP.Volume - VolumeIncrement, MinVolume);
            }
        }

        /// <summary>
        /// This method toggles position of seekbar with keyboard shortcuts forward and backward
        /// </summary>
        /// <param name="forward">if forward is true then seek forward else seek backward</param>
        public void TogglePosition(bool forward)
        {
            var coreWindow = Window.Current.CoreWindow;
            var stepSeconds = SmallStepSeconds;

            if (coreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
            {
                stepSeconds = MediumStepSeconds;
            }

            if (coreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
            {
                stepSeconds = LargeStepSeconds;
            }

            var timeSpan = TimeSpan.FromSeconds(stepSeconds);
            MP.PlaybackSession.Position += forward ? timeSpan : -timeSpan;
        }

        /// <summary>
        /// Toggle Mute with Keyboard Shortcuts
        /// </summary>
        /// <returns>After toggle whether Mute is true or not</returns>
        public bool ToggleMute()
        {
            MP.IsMuted = !MP.IsMuted;
            return MP.IsMuted;
        }

        /// <summary>
        /// Toggle Play/Pause with KeyboardShortCuts
        /// </summary>
        public void TogglePlayPause()
        {
            if (MP.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }
        #endregion KeyboardShortcuts_Methods
    }
}
