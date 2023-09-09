using System;
using System.Threading.Tasks;
using Fluent_Video_Player.Database;
using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Services;
using Fluent_Video_Player.ViewModels;
using Microsoft.AppCenter.Analytics;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.Media.Playback;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using static Fluent_Video_Player.Helpers.MediaHelper;
namespace Fluent_Video_Player.Views
{
    public sealed partial class PlayerPage : Page
    {
        public PlayerViewModel ViewModel { get; } = new PlayerViewModel();

        // For more on the MediaPlayer and adjusting controls and behavior see https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/media-playback
        // The DisplayRequest is used to stop the screen dimming while watching for extended periods
        private DisplayRequest _displayRequest = new DisplayRequest();
        private bool _isRequestActive = false;
        private CoreCursor cursor;
        private ShellViewModel _shellVM => DataContext as ShellViewModel;
        public static PlayerViewModel VM;
        public PlayerPage()
        {
            InitializeComponent();
            ViewModel.SecondGrid = SecondGrid;
            VM = ViewModel;
            ViewModel.Initialize();
        }

        #region Events

        private async void Fluentmtc_OpenButtonClicked(object sender, EventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            foreach (var type in FileHelper.VideoFileTypes)
                picker.FileTypeFilter.Add(type);

            var files = await picker.PickMultipleFilesAsync();
            await MyMediaPlayer.SetNewSourceWithFiles(files);
            ViewModel.Initialize();
        }

        private async void MP_VolumeChanged(MediaPlayer sender, object args)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                if (Fluentmtc.VolumeIndicator != default(object))
                    Fluentmtc.VolumeIndicator.Value = MyMediaPlayer.MP.Volume;
            });
        }

        private async void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                if (!(Fluentmtc.MiniSeekBar is null))
                {
                    Fluentmtc.MiniSeekBar.Maximum = MyMediaPlayer.MP.PlaybackSession.NaturalDuration.TotalMilliseconds;
                    Fluentmtc.MiniSeekBar.Minimum = 0;
                    Fluentmtc.MiniSeekBar.Value = MyMediaPlayer.MP.PlaybackSession.Position.TotalMilliseconds;
                }
            });
        }

        //private void LikedIconUpdate()
        //{
        //    var icon = (FontIcon)(Fluentmtc.LikeButton.Icon);
        //    if (MyMediaPlayer.CurrentVideoIsLiked)
        //    {
        //        ToolTipService.SetToolTip(Fluentmtc.LikeButton, "Remove From Likes");//get from localized values
        //        icon.Glyph = "B";
        //    }
        //    else
        //    {
        //        ToolTipService.SetToolTip(Fluentmtc.LikeButton, "Add To Likes");//get from localized values
        //        icon.Glyph = "A";
        //    }
        //}

        //private async void Fluentmtc_LikeToggled(object sender, LikeToggledEventArgs e)
        //{
        //    switch (e.Value)
        //    {
        //        case LikeToggledValue.Liked:
        //            if (await MyMediaPlayer.LikeCurrentVideo())
        //                LikedInAppNotification.Show("Added To Likes", 3000);//get from localized values
        //            else
        //                LikedInAppNotification.Show("Your Likes are already full! Please remove some to add more.", 4000);//get from locazlied values
        //            break;
        //        case LikeToggledValue.Unliked:
        //            await MyMediaPlayer.UnlikeCurrentVideo();
        //            LikedInAppNotification.Show("Removed From Likes", 3000);//get from localized values
        //            break;
        //    }
        //    LikedIconUpdate();
        //}

        /// <summary>
        /// event to deal with audio device changing in the combox of the controls
        /// </summary>
        private void Fluentmtc_AudioDeviceChanged(object sender, AudioDeviceChangedEventArgs e)
        {
            try
            {
                if (e.SelectedIndex != -1 && MyMediaPlayer.MP.AudioDevice != e.AudioDevice)
                    MyMediaPlayer.MP.AudioDevice = e.AudioDevice;
            }
            catch { }
        }

        private async void Fluentmtc_PipToggled(object sender, EventArgs e) => await PipMode();

        private async void MyMediaPlayer_CurrentItemOpened(object sender, CurrentItemOpenedEventArgs e)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                if (!(e?.ResumePosition is null))
                {
                    if (e.ResumePosition == DbHelper.LastRetrievedPosition && DbHelper.LastRetrievedPosition != TimeSpan.Zero && CurrentView.ViewMode == ApplicationViewMode.Default)
                    {
                        await Task.Delay(2000);
                        ResumeInAppNotification.Show();
                        await Task.Delay(4000);
                        ResumeInAppNotification.Dismiss();
                    }
                }
                if (!(Fluentmtc?.CurrentTextBlock is null))
                    Fluentmtc.CurrentTextBlock.Text = e.CurrentTitle;
            });
        }

        private void ResumeYesClicked(object sender, RoutedEventArgs e)
        {
            ResumeInAppNotification.Dismiss();
            MyMediaPlayer.MP.PlaybackSession.Position = DbHelper.LastRetrievedPosition;
        }
        private void ResumeNoClicked(object sender, RoutedEventArgs e) => ResumeInAppNotification.Dismiss();

        private async void Fluentmtc_LoopingToggled(object sender, LoopingToggledEventArgs e)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                MyMediaPlayer.CurrentPlaybackList.AutoRepeatEnabled = e.IsChecked;
                var looping = MyMediaPlayer.CheckLooping();
                Fluentmtc.IsPreviousTrackButtonVisible = looping.Item1;
                Fluentmtc.IsNextTrackButtonVisible = looping.Item2;
                MyMediaPlayer.MP.SystemMediaTransportControls.IsPreviousEnabled = Fluentmtc.IsPreviousTrackButtonVisible;
                MyMediaPlayer.MP.SystemMediaTransportControls.IsNextEnabled = Fluentmtc.IsNextTrackButtonVisible;

            });
        }

        private async void MP_MediaOpened(object sender, EventArgs args)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                if (!(Fluentmtc.LoopButton is null))
                    Fluentmtc.LoopButton.IsChecked = false;
            });
        }


        private async void MyMediaPlayer_CurrentPlaybackListItemChanged(object sender, CurrentPlaybackListItemChangedEventArgs e)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                Fluentmtc.IsPreviousTrackButtonVisible = e.PreviousButtonEnabled;
                Fluentmtc.IsNextTrackButtonVisible = e.NextButtonEnabled;
                MyMediaPlayer.MP.SystemMediaTransportControls.IsPreviousEnabled = Fluentmtc.IsPreviousTrackButtonVisible;
                MyMediaPlayer.MP.SystemMediaTransportControls.IsNextEnabled = Fluentmtc.IsNextTrackButtonVisible;
                if (PlaylistList.SelectedItem != MyMediaPlayer?.CurrentPlaybackList?.CurrentItem)
                    PlaylistList.SelectedItem = MyMediaPlayer?.CurrentPlaybackList?.CurrentItem;
            });
        }
        #endregion
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            base.OnNavigatedTo(e);
            Analytics.TrackEvent("PlayerPage.OnNavigatedTo()");

            var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("MiniConnectedAnimation");
            if (!(anim is null))
                anim.TryStart(mpe);

            if (mpe.MediaPlayer != MyMediaPlayer.MP)
                mpe.SetMediaPlayer(MyMediaPlayer.MP);

            MyMediaPlayer.MP.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
            MyMediaPlayer.MP.VolumeChanged += MP_VolumeChanged;
            MyMediaPlayer.InitialItemOpened += MP_MediaOpened;
            MyMediaPlayer.CurrentItemOpened += MyMediaPlayer_CurrentItemOpened;
            MyMediaPlayer.MP.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            Fluentmtc.PipToggled += Fluentmtc_PipToggled;
            Fluentmtc.AudioDeviceChanged += Fluentmtc_AudioDeviceChanged;
            Fluentmtc.OpenButtonClicked += Fluentmtc_OpenButtonClicked;
            Fluentmtc.LoopingToggled += Fluentmtc_LoopingToggled;
            Fluentmtc.ControlFadeChanged += Fluentmtc_ControlFadeChanged;
            Fluentmtc.FullWindowButtonClicked += Fluentmtc_FullWindowButtonClicked;
            Fluentmtc.PlaylistDoneClicked += Fluentmtc_PlaylistDoneClicked;
            MyMediaPlayer.CurrentPlaybackListItemChanged += MyMediaPlayer_CurrentPlaybackListItemChanged;
            //Fluentmtc.LikeToggled += Fluentmtc_LikeToggled;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            _shellVM.DeactivateMiniPlayer();
        }

        private void Fluentmtc_PlaylistDoneClicked(object sender, EventArgs e) => _shellVM.AddNewPlaylistNavigationItem(Fluentmtc.PlaylistBox.Text);

        private void Fluentmtc_FullWindowButtonClicked(object sender, EventArgs e)
        {
            if (mpe.IsFullWindow)
                DefaultOrFullScreen();
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Analytics.TrackEvent("PlayerPage.OnNavigatedFrom()");
            MyMediaPlayer.MP.PlaybackSession.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;
            MyMediaPlayer.MP.VolumeChanged -= MP_VolumeChanged;
            MyMediaPlayer.InitialItemOpened -= MP_MediaOpened;
            MyMediaPlayer.MP.PlaybackSession.PositionChanged -= PlaybackSession_PositionChanged;
            Fluentmtc.PipToggled -= Fluentmtc_PipToggled;
            Fluentmtc.OpenButtonClicked -= Fluentmtc_OpenButtonClicked;
            MyMediaPlayer.CurrentItemOpened -= MyMediaPlayer_CurrentItemOpened;
            Fluentmtc.LoopingToggled -= Fluentmtc_LoopingToggled;
            Fluentmtc.ControlFadeChanged -= Fluentmtc_ControlFadeChanged;
            Fluentmtc.FullWindowButtonClicked -= Fluentmtc_FullWindowButtonClicked;
            Fluentmtc.PlaylistDoneClicked -= Fluentmtc_PlaylistDoneClicked;
            MyMediaPlayer.CurrentPlaybackListItemChanged -= MyMediaPlayer_CurrentPlaybackListItemChanged;
            //Fluentmtc.LikeToggled -= Fluentmtc_LikeToggled;
            Fluentmtc.AudioDeviceChanged -= Fluentmtc_AudioDeviceChanged;
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;

            if (mpe.MediaPlayer.PlaybackSession.PlaybackState != MediaPlaybackState.None)
            {
                _shellVM.ActivateMiniPlayer();
            }
            //MyMediaPlayer.Pause();
            //deal with crusor bug
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                if (cursor is null)
                {
                    cursor = new CoreCursor(CoreCursorType.Arrow, 1);
                }
                if (!(Window.Current is null))
                {
                    Window.Current.CoreWindow.PointerCursor = cursor;
                    cursor = null;
                }
            });
        }

        private void Fluentmtc_ControlFadeChanged(object sender, ControlFadeChangedEventArgs e)
        {
            if (e.Appeared)
                ToggleCursor(true);
            else if (mpe.IsFullWindow)
                ToggleCursor(false);
        }



        /// <summary>
        /// The event to manage the keyboard shortcuts on this player page.
        /// </summary>
        private async void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (Fluentmtc.PlaylistBox.FocusState == FocusState.Unfocused)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.Up:
                        Fluentmtc.VolumeIndicatorAppear(true);
                        MyMediaPlayer.ToggleVolume(true); Fluentmtc.VolumeIndicatorDisAppear(); break;
                    case VirtualKey.Down:
                        Fluentmtc.VolumeIndicatorAppear(false);
                        MyMediaPlayer.ToggleVolume(false); Fluentmtc.VolumeIndicatorDisAppear(); break;
                    case VirtualKey.Right: MyMediaPlayer.TogglePosition(true); break;
                    case VirtualKey.Left: MyMediaPlayer.TogglePosition(false); break;
                    case VirtualKey.D: await MyMediaPlayer.CurrentPlaybackList.MoveNext(MediaPlaybackItemChangedReason.AppRequested); break;
                    case VirtualKey.A: await MyMediaPlayer.CurrentPlaybackList.MovePrevious(MediaPlaybackItemChangedReason.AppRequested); break;
                    case VirtualKey.F:
                    case VirtualKey.Escape: ToggleFullScreen(); break;
                    case VirtualKey.M: MyMediaPlayer.ToggleMute(); break;
                    case VirtualKey.L: Fluentmtc.LoopButton.IsChecked = !Fluentmtc.LoopButton.IsChecked; break;
                    case VirtualKey.P: await PipMode(); break;
                    case VirtualKey.B: Fluentmtc.MiniBarToggleButton.IsChecked = !Fluentmtc.MiniBarToggleButton.IsChecked; break;
                    case VirtualKey.Space: MyMediaPlayer.TogglePlayPause(); break;
                }
            }
        }
        ApplicationView CurrentView => ApplicationView.GetForCurrentView();
        /// <summary>
        /// This method toggles the pip mode with keyboard shortcut "P"
        /// </summary>
        private async Task PipMode()
        {
            bool changed = CurrentView.ViewMode == ApplicationViewMode.CompactOverlay ?
                           await CurrentView.TryEnterViewModeAsync(ApplicationViewMode.Default) :
                           await CurrentView.TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
            if (changed)
            {
                if (CurrentView.ViewMode == ApplicationViewMode.CompactOverlay)
                {
                    Fluentmtc.ExtraControls.Visibility = Visibility.Collapsed;
                    _shellVM.IsBackVisible = false;
                    _shellVM.IsPaneButtonVisible = false;
                    Fluentmtc.VolumeInternalBorder.Height = 42.0;
                    Fluentmtc.VolumeInternalBorder.Width = 50.0;
                    Fluentmtc.VolumeIcon.Width = 20.0;
                    Fluentmtc.VolumeIcon.Height = 20.0;
                    Fluentmtc.VolumeIndicator.Height = 4.0;
                    Fluentmtc.VolumeIndicatorBorder.BlurRadius = 10.0;
                    ViewModel.SecondGridVisibility = false;
                    _shellVM.TurnToLeftModeWithoutSaving();
                    (_shellVM.NavigationView.FindDescendantByName("HeaderContent")).MinHeight = 0.0;
                    _shellVM.AppNameHeight = 0.0;
                    _shellVM.NavigationView.Header = null;//only setting it to null bcz when going to normal or full mode
                    // _shellVM.CheckDisplayMode(); does the job for setting required header
                }
                else { DefaultOrFullScreen(); }
            }
        }

        private void DefaultOrFullScreen()
        {
            if (Fluentmtc?.ControlsFade?.CurrentState?.Name == "ControlPanelFadeOut")
                ToggleCursor(false);

            Fluentmtc.ExtraControls.Visibility = Visibility.Visible;
            _shellVM.IsBackVisible = true;
            _shellVM.IsPaneButtonVisible = true;
            Fluentmtc.VolumeInternalBorder.Height = 84.0;
            Fluentmtc.VolumeInternalBorder.Width = 100.0;
            Fluentmtc.VolumeIcon.Width = 40.0;
            Fluentmtc.VolumeIcon.Height = 40.0;
            Fluentmtc.VolumeIndicator.Height = 8.0;
            Fluentmtc.VolumeIndicatorBorder.BlurRadius = 20.0;
            (_shellVM.NavigationView.FindDescendantByName("HeaderContent")).MinHeight = 40.0;
            _shellVM.AppNameHeight = 32.0;
            _shellVM.CheckDisplayMode();

            if (MediaHelper.MyMediaPlayer?.CurrentPlaybackList?.Items?.Count > 1 &&
               ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.Default)
            { ViewModel.SecondGridVisibility = true; }
        }

        /// <summary>
        /// Event for when the the main MediaPlayer changes its playback session
        /// </summary>
        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            try
            {
                if (sender is MediaPlaybackSession playbackSession && playbackSession.NaturalVideoHeight != 0)
                {
                    if (playbackSession.PlaybackState == MediaPlaybackState.Playing)
                    {
                        if (!_isRequestActive)
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                try
                                {
                                    _displayRequest.RequestActive(); _isRequestActive = true;
                                }
                                catch { }
                            });
                        }
                    }
                    else
                    {
                        if (_isRequestActive)
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                try
                                {
                                    _displayRequest.RequestRelease(); _isRequestActive = false;
                                }
                                catch { }
                            });
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Event for when this page (PlayerPage) gets loaded.
        /// </summary>
        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ApplicationViewExtensions.SetExtendViewIntoTitleBar(this, true);
            TitleBarExtensions.SetButtonBackgroundColor(this, Colors.Transparent);
            SetTitleBar();
        }

        /// <summary>
        /// Event for settings the title bar extended on Desktop.
        /// </summary>
        private void SetTitleBar()
        {
            RequestedTheme = ThemeSelectorService.Theme;
            switch (ThemeSelectorService.Theme)
            {
                case Windows.UI.Xaml.ElementTheme.Default:
                case Windows.UI.Xaml.ElementTheme.Light:
                    TitleBarExtensions.SetButtonForegroundColor(this, Colors.Black);
                    break;
                case Windows.UI.Xaml.ElementTheme.Dark:
                    TitleBarExtensions.SetButtonForegroundColor(this, Colors.White);
                    break;
            }
        }


        private void Fluentmtc_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //deal with like icon
            //LikedIconUpdate();
            //adjust the volume for start.
            Fluentmtc.VolumeIndicator.Value = MyMediaPlayer.MP.Volume;
            Fluentmtc.LoopButton.IsChecked = MyMediaPlayer.CurrentPlaybackList.AutoRepeatEnabled;
            if (MyMediaPlayer.CurrentPlaybackList.Items.Count > 0 && MyMediaPlayer.CurrentPlaybackList.CurrentItem != default(object))
            {
                Fluentmtc.CurrentTextBlock.Text = MyMediaPlayer.CurrentPlaybackList.CurrentItem.GetDisplayProperties().VideoProperties.Title;
            }
        }

        private void Fluentmtc_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e) => ToggleFullScreen();

        private void ToggleFullScreen()
        {
            mpe.IsFullWindow = !mpe.IsFullWindow;
            if (mpe.IsFullWindow)
                DefaultOrFullScreen();
        }

        private void ToggleCursor(bool v)
        {
            if (!v)
            {
                //disappear cursor
                if (!(Window.Current is null))
                {
                    cursor = Window.Current.CoreWindow.PointerCursor;
                    Window.Current.CoreWindow.PointerCursor = null;
                }
            }
            else
            {
                if (cursor is null)
                    cursor = new CoreCursor(CoreCursorType.Arrow, 1);
                if (!(Window.Current is null))
                {
                    Window.Current.CoreWindow.PointerCursor = cursor;
                    cursor = null;
                }
            }
        }

        private async void PlaylistSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaylistList.SelectedIndex != -1)
            {
                var item = PlaylistList.SelectedItem as FluentPlaybackItem;
                try
                {                    
                    var thumbnail = PlaylistList.ContainerFromItem(item) as UIElement;
                    if (!(thumbnail is null))
                        ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ThumbnailConnectedAnimation", thumbnail);
                    var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("ThumbnailConnectedAnimation");
                    ViewModel.PosterSource = item.MyVideo.Thumbnail;

                    if (!(anim is null))
                        anim.TryStart(mpe);
                    await MyMediaPlayer.CurrentPlaybackList.MoveToItem(item);
                    PlaylistList.ScrollIntoView(item, ScrollIntoViewAlignment.Leading);
                }
                catch (System.IO.FileNotFoundException)
                {
                    MyMediaPlayer.CurrentPlaybackList.Items.Remove(item);
                    ViewModel.Source.Remove(item);
                }
            }
        }

    }
}
