using System;
using System.Collections.Generic;
using System.Linq;

using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Services;
using Fluent_Video_Player.Views;
using MahApps.Metro.IconPacks;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using static Fluent_Video_Player.Helpers.MediaHelper;
using WinUI = Microsoft.UI.Xaml.Controls;
using AppData = Windows.Storage.ApplicationData;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Fluent_Video_Player.ViewModels
{
    public partial class ShellViewModel : ObservableObject
    {
        #region fields
        private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);
        private readonly PackIconMaterialDesignKind _topIconKind = PackIconMaterialDesignKind.ArrowUpward;
        private readonly PackIconMaterialDesignKind _leftIconKind = PackIconMaterialDesignKind.ArrowDownward;
        private IList<KeyboardAccelerator> _keyboardAccelerators;
        private MediaPlayerElement _mpe;
        private FluentMtc _fluentmtc;
        private Grid _miniPlayerGrid;
        private Button _displayModeButton;
        private InAppNotification _ratingInAppNotification;
        private ShellPage _myPage;
        private WinUI.NavigationView _navigationView;
        #endregion fields

        #region Props
        public WinUI.NavigationView NavigationView => _navigationView;
        #endregion Props

        #region ObservableFields
        [ObservableProperty] private PackIconMaterialDesignKind displayModeIconKind;
        [ObservableProperty] private bool isDisplayModeBlockVisible;
        [ObservableProperty] private bool isMiniPlayerVisible;
        [ObservableProperty] private bool isBackEnabled;
        [ObservableProperty] private WinUI.NavigationViewItem selected;
        [ObservableProperty] private bool isBackVisible;
        [ObservableProperty] private bool isPaneButtonVisible;
        [ObservableProperty] private double appNameHeight = 32.0;
        #endregion ObservableFields

        #region DisplayMode
        private void OnDisplayModeChangedByUser() => ReverseDisplayMode();
        [RelayCommand] private void DisplayModeLoaded() => CheckDisplayMode();
        public void CheckDisplayMode()
        {
            switch (AppData.Current.LocalSettings.ReadDisplayMode())
            {
                case CurrentDisplayMode.LeftMode:
                    TurnToLeftMode();
                    break;
                case CurrentDisplayMode.TopMode:
                    TurnToTopMode();
                    break;
            }
        }
        private void ReverseDisplayMode()
        {
            switch (AppData.Current.LocalSettings.ReadDisplayMode())
            {
                case CurrentDisplayMode.LeftMode:
                    TurnToTopMode();
                    break;
                case CurrentDisplayMode.TopMode:
                    TurnToLeftMode();
                    break;
            }
        }
        private void TurnToLeftMode()
        {
            TurnToLeftModeWithoutSaving();
            AppData.Current.LocalSettings.SaveString(SettingsStorageExtensions.DisplayModeKey, CurrentDisplayMode.LeftMode.ToString());
        }
        public void TurnToLeftModeWithoutSaving()
        {
            _navigationView.PaneDisplayMode = WinUI.NavigationViewPaneDisplayMode.Auto;
            DisplayModeIconKind = _topIconKind;
            IsDisplayModeBlockVisible = true;
        }
        private void TurnToTopMode()
        {
            _navigationView.PaneDisplayMode = WinUI.NavigationViewPaneDisplayMode.Top;
            DisplayModeIconKind = _leftIconKind;
            IsDisplayModeBlockVisible = false;
            AppData.Current.LocalSettings.SaveString(SettingsStorageExtensions.DisplayModeKey, CurrentDisplayMode.TopMode.ToString());
        }
        #endregion

        #region CommandMethods
        [RelayCommand]
        private async Task RatingYesAsync()
        {
            _ratingInAppNotification.Dismiss();
            await FileHelper.Rate();
        }
        [RelayCommand] private void RatingNo() => _ratingInAppNotification.Dismiss();
        private void OnCloseMini() { MyMediaPlayer.Pause(); DeactivateMiniPlayer(); }
        [RelayCommand]
        private async Task LoadedAsync()
        {
            ThemeSelectorService.ThemeToggled += ThemeSelectorService_ThemeToggled;
            SetTitleBar();
            _keyboardAccelerators.Add(_altLeftKeyboardAccelerator); _keyboardAccelerators.Add(_backKeyboardAccelerator);

            await LoadPlaylists();
            await AskForRating();
        }

        [RelayCommand] private void UnLoaded() => ThemeSelectorService.ThemeToggled -= ThemeSelectorService_ThemeToggled;
        private void ThemeSelectorService_ThemeToggled(object sender, EventArgs e) => SetTitleBar();
        [RelayCommand]
        private void ItemInvoked(WinUI.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                NavigationService.Navigate(typeof(SettingsPage));
                return;
            }
            try
            {
                var item = _navigationView.MenuItems
                                .OfType<WinUI.NavigationViewItem>()
                                .First(menuItem => (string)menuItem.Content == (string)args.InvokedItem);
                var pageType = item.GetValue(NavHelper.NavigateToProperty) as Type;
                if (pageType == typeof(PlayerPage) && IsMiniPlayerVisible)
                {
                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("MiniConnectedAnimation", _miniPlayerGrid);
                    NavigationService.Navigate(pageType, infoOverride: new SuppressNavigationTransitionInfo());
                    return;
                }
                if (pageType == typeof(PlaylistPage))
                {
                    NavigationService.Navigate(pageType, item.Content, navigateToSamePage: true);
                    return;
                }
                NavigationService.Navigate(pageType);
            }
            catch { }
        }
        private void OnBackRequested(WinUI.NavigationView sender, WinUI.NavigationViewBackRequestedEventArgs args) => NavigationService.GoBack();
        private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var result = NavigationService.GoBack();
            args.Handled = result;
        }
        #endregion

        #region Startup
        public ShellViewModel() { }
        public void Initialize(ShellPage myPage, Frame frame, WinUI.NavigationView navigationView, IList<KeyboardAccelerator> keyboardAccelerators,
            MediaPlayerElement mpe, FluentMtc fluentmtc, Grid miniPlayerGrid, Button displayModeButton, InAppNotification ratingInAppNotification)
        {
            _myPage = myPage;
            _navigationView = navigationView;
            _keyboardAccelerators = keyboardAccelerators;
            _mpe = mpe;
            _fluentmtc = fluentmtc;
            _miniPlayerGrid = miniPlayerGrid;
            _displayModeButton = displayModeButton;
            _ratingInAppNotification = ratingInAppNotification;
            NavigationService.Frame = frame;
            NavigationService.Navigated += Frame_Navigated;
            _navigationView.BackRequested += OnBackRequested;
            IsBackVisible = true;
            IsPaneButtonVisible = true;
            IsMiniPlayerVisible = false;
            SetHeader();
        }
        [RelayCommand] private void DisplayModeChanged() => SetHeader();
        [RelayCommand] private void DisplayModeChange() => OnDisplayModeChangedByUser();
        private void SetHeader()
        {
            if ((_navigationView.PaneDisplayMode == WinUI.NavigationViewPaneDisplayMode.LeftMinimal ||
                _navigationView.PaneDisplayMode == WinUI.NavigationViewPaneDisplayMode.Auto) &&
                _navigationView.CompactModeThresholdWidth > CoreWindow.GetForCurrentThread().Bounds.Width)
                _navigationView.Header = "";
            else
                _navigationView.Header = null;
        }
        private async Task LoadPlaylists()
        {
            var playlists = await Database.DbHelper.GetPlaylists();
            foreach (var playlist in playlists)
            {
                AddNewPlaylistNavigationItem(playlist);
            }
        }
        internal void AddNewPlaylistNavigationItem(string playlist)
        {
            var playlistIcon = new PathIconMaterialDesign { Kind = PackIconMaterialDesignKind.PlaylistPlay };
            var item = new WinUI.NavigationViewItem { Content = playlist, Icon = playlistIcon, Tag = Constants.PlaylistTag };
            NavHelper.SetNavigateTo(item, typeof(PlaylistPage));
            _navigationView.MenuItems.Add(item);
        }
        #endregion

        #region OtherMethods
        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            IsBackEnabled = NavigationService.CanGoBack;
            if (e.SourcePageType == typeof(SettingsPage))
            {
                Selected = _navigationView.SettingsItem as WinUI.NavigationViewItem;
                return;
            }

            Selected = _navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.SourcePageType));
        }
        private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType)
        {
            var pageType = menuItem.GetValue(NavHelper.NavigateToProperty) as Type;
            return pageType == sourcePageType;
        }
        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
            return keyboardAccelerator;
        }
        private async Task AskForRating()
        {
            bool hasRated = false;
            hasRated = await AppData.Current.RoamingSettings.ReadAsync<bool>(nameof(hasRated));
            if (!SystemInformation.IsFirstRun && SystemInformation.TotalLaunchCount % 5 == 0 && !hasRated)
            {
                await Task.Delay(2000);
                _ratingInAppNotification.Show();
                await Task.Delay(6000);
                _ratingInAppNotification.Dismiss();
            }
        }
        private void SetTitleBar()
        {
            if (DeviceHelper.GetDevice() == DeviceHelper.Device.Desktop)
            {
                ApplicationViewExtensions.SetExtendViewIntoTitleBar(_myPage, true);
                TitleBarExtensions.SetButtonBackgroundColor(_myPage, Colors.Transparent);
                switch (ThemeSelectorService.Theme)
                {
                    case Windows.UI.Xaml.ElementTheme.Default:
                        if (Application.Current.RequestedTheme == ApplicationTheme.Light)
                            TitleBarExtensions.SetButtonForegroundColor(_myPage, Colors.Black);
                        else { TitleBarExtensions.SetButtonForegroundColor(_myPage, Colors.White); }
                        break;
                    case Windows.UI.Xaml.ElementTheme.Light:
                        TitleBarExtensions.SetButtonForegroundColor(_myPage, Colors.Black); break;
                    case Windows.UI.Xaml.ElementTheme.Dark:
                        TitleBarExtensions.SetButtonForegroundColor(_myPage, Colors.White); break;
                }
            }
        }
        #endregion

        #region MediaStuff  
        internal void DeactivateMiniPlayer()
        {
            IsMiniPlayerVisible = false;

            MyMediaPlayer.MP.PlaybackSession.PositionChanged -= PlaybackSession_PositionChanged;
            MyMediaPlayer.MP.VolumeChanged -= MP_VolumeChanged;
            MyMediaPlayer.InitialItemOpened -= MP_MediaOpened;
            _fluentmtc.AudioDeviceChanged -= Fluentmtc_AudioDeviceChanged;
            _fluentmtc.LoopingToggled -= Fluentmtc_LoopingToggled;
            _fluentmtc.CloseButtonClicked -= _fluentmtc_CloseButtonClicked;
            _fluentmtc.GoToPlayerPageButtonClicked -= _fluentmtc_GoToPlayerPageButtonClicked;
            MyMediaPlayer.CurrentPlaybackListItemChanged -= MyMediaPlayer_CurrentPlaybackListItemChanged;
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
        }
        internal void ActivateMiniPlayer()
        {
            IsMiniPlayerVisible = true;
            if (!(_fluentmtc.CurrentTextBlock is null))
            {
                _fluentmtc.CurrentTextBlock.Text = MyMediaPlayer?.CurrentPlaybackList?.CurrentItem?.MyVideo?.Title;
                ToolTipService.SetToolTip(_fluentmtc.CurrentTextBlock, _fluentmtc.CurrentTextBlock.Text);
            }

            _mpe.SetMediaPlayer(MyMediaPlayer.MP);

            MyMediaPlayer.MP.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
            MyMediaPlayer.MP.VolumeChanged += MP_VolumeChanged;
            MyMediaPlayer.InitialItemOpened += MP_MediaOpened;
            _fluentmtc.AudioDeviceChanged += Fluentmtc_AudioDeviceChanged;
            _fluentmtc.LoopingToggled += Fluentmtc_LoopingToggled;
            _fluentmtc.CloseButtonClicked += _fluentmtc_CloseButtonClicked;
            _fluentmtc.GoToPlayerPageButtonClicked += _fluentmtc_GoToPlayerPageButtonClicked;
            MyMediaPlayer.CurrentPlaybackListItemChanged += MyMediaPlayer_CurrentPlaybackListItemChanged;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        private void _fluentmtc_GoToPlayerPageButtonClicked(object sender, EventArgs e)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("MiniConnectedAnimation", _miniPlayerGrid);
            NavigationService.Navigate<PlayerPage>(infoOverride: new SuppressNavigationTransitionInfo());
        }

        private void _fluentmtc_CloseButtonClicked(object sender, EventArgs e) => OnCloseMini();
        private async void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                if (!(_fluentmtc.MiniSeekBar is null))
                {
                    _fluentmtc.MiniSeekBar.Maximum = MyMediaPlayer.MP.PlaybackSession.NaturalDuration.TotalMilliseconds;
                    _fluentmtc.MiniSeekBar.Minimum = 0;
                    _fluentmtc.MiniSeekBar.Value = MyMediaPlayer.MP.PlaybackSession.Position.TotalMilliseconds;
                }
            });
        }
        private async void MP_VolumeChanged(MediaPlayer sender, object args)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                if (!(_fluentmtc.VolumeIndicator is null))
                    _fluentmtc.VolumeIndicator.Value = MyMediaPlayer.MP.Volume;
            });
        }
        private async void MP_MediaOpened(object sender, EventArgs args) => await DispatcherHelper.ExecuteOnUIThreadAsync(() => _fluentmtc.LoopButton.IsChecked = false);
        private void Fluentmtc_AudioDeviceChanged(object sender, AudioDeviceChangedEventArgs e)
        {
            try
            {
                if (e.SelectedIndex != -1 && MyMediaPlayer.MP.AudioDevice != e.AudioDevice)
                    MyMediaPlayer.MP.AudioDevice = e.AudioDevice;
            }
            catch { }
        }
        private async void Fluentmtc_LoopingToggled(object sender, LoopingToggledEventArgs e)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                MyMediaPlayer.CurrentPlaybackList.AutoRepeatEnabled = e.IsChecked;
                var looping = MyMediaPlayer.CheckLooping();
                _fluentmtc.IsPreviousTrackButtonVisible = looping.Item1;
                _fluentmtc.IsNextTrackButtonVisible = looping.Item2;
                MyMediaPlayer.MP.SystemMediaTransportControls.IsPreviousEnabled = _fluentmtc.IsPreviousTrackButtonVisible;
                MyMediaPlayer.MP.SystemMediaTransportControls.IsNextEnabled = _fluentmtc.IsNextTrackButtonVisible;

            });
        }
        private async void MyMediaPlayer_CurrentPlaybackListItemChanged(object sender, CurrentPlaybackListItemChangedEventArgs e)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                _fluentmtc.IsPreviousTrackButtonVisible = e.PreviousButtonEnabled;
                _fluentmtc.IsNextTrackButtonVisible = e.NextButtonEnabled;
                MyMediaPlayer.MP.SystemMediaTransportControls.IsPreviousEnabled = _fluentmtc.IsPreviousTrackButtonVisible;
                MyMediaPlayer.MP.SystemMediaTransportControls.IsNextEnabled = _fluentmtc.IsNextTrackButtonVisible;
            });
        }
        private async void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            var box = Controls.FluentGridView.Box?.FindDescendant<TextBox>();
            if (box?.FocusState == FocusState.Unfocused)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.Up:
                        _fluentmtc.VolumeIndicatorAppear(true);
                        MyMediaPlayer.ToggleVolume(true); _fluentmtc.VolumeIndicatorDisAppear(); break;
                    case VirtualKey.Down:
                        _fluentmtc.VolumeIndicatorAppear(false);
                        MyMediaPlayer.ToggleVolume(false); _fluentmtc.VolumeIndicatorDisAppear(); break;
                    case VirtualKey.Right:
                        MyMediaPlayer.TogglePosition(true); break;
                    case VirtualKey.Left:
                        MyMediaPlayer.TogglePosition(false); break;
                    case VirtualKey.D:
                        await MyMediaPlayer.CurrentPlaybackList.MoveNext(MediaPlaybackItemChangedReason.AppRequested); break;
                    case VirtualKey.A:
                        await MyMediaPlayer.CurrentPlaybackList.MovePrevious(MediaPlaybackItemChangedReason.AppRequested); break;
                    case VirtualKey.M:
                        MyMediaPlayer.ToggleMute(); break;
                    case VirtualKey.L:
                        _fluentmtc.LoopButton.IsChecked = !_fluentmtc.LoopButton.IsChecked; break;
                    case VirtualKey.B:
                        _fluentmtc.MiniBarToggleButton.IsChecked = !_fluentmtc.MiniBarToggleButton.IsChecked; break;
                    case VirtualKey.Space:
                        MyMediaPlayer.TogglePlayPause(); break;
                    case VirtualKey.Delete:
                        OnCloseMini(); break;
                }
            }
        }
        #endregion
    }
}

