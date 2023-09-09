using Fluent_Video_Player.Database;
using Fluent_Video_Player.Helpers;
using MahApps.Metro.IconPacks;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Devices;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluent_Video_Player
{
    public class LoopingToggledEventArgs : EventArgs
    {
        public bool IsChecked { get; set; }
    }
    public class AudioDeviceChangedEventArgs : EventArgs
    {
        public int SelectedIndex { get; set; } = 0;
        public DeviceInformation AudioDevice { get; set; }
    }
    //public enum LikeToggledValue
    //{
    //    Liked, Unliked
    //}
    //public class LikeToggledEventArgs : EventArgs
    //{
    //    public LikeToggledEventArgs(LikeToggledValue value) => Value = value;
    //    public LikeToggledValue Value { get; }
    //}
    public sealed class FluentMtc : MediaTransportControls
    {
        #region events
        //public event EventHandler<LikeToggledEventArgs> LikeToggled;
        public event EventHandler<EventArgs> PipToggled;
        public event EventHandler<EventArgs> OpenButtonClicked;
        public event EventHandler<EventArgs> CloseButtonClicked;
        public event EventHandler<EventArgs> PlaylistDoneClicked;
        public event EventHandler<EventArgs> GoToPlayerPageButtonClicked;
        public event EventHandler<EventArgs> FullWindowButtonClicked;
        public event EventHandler<ControlFadeChangedEventArgs> ControlFadeChanged;
        public event EventHandler<AudioDeviceChangedEventArgs> AudioDeviceChanged;
        public event EventHandler<LoopingToggledEventArgs> LoopingToggled;

        private Flyout _playlistFlyout;
        #endregion

        #region Fields

        public Border VolumeInternalBorder { get; private set; }
        public PackIconMaterialDesign VolumeIcon { get; private set; }
        public VisualStateGroup ControlsFade { get; private set; }

        private Slider _progressSlider;
        private Slider _volumeSlider;
        private ComboBox _audioDeviceComboBox;
        private List<DeviceInformation> _currentDevices;

        public AppBarButton CloseButton { get; private set; }
        public AppBarButton AddToPlaylistButton { get; private set; }
        public AppBarButton GoToPlayerPageButton { get; private set; }

        private Flyout _audioDeviceFlyout;

        public ProgressBar VolumeIndicator { get; private set; }
        public Grid ExtraControls { get; private set; }
        public AppBarButton PipButton { get; private set; }
        public DropShadowPanel VolumeIndicatorBorder { get; private set; }

        //private AppBarButton _likeButton;
        #endregion

        #region publicProps
        //public AppBarButton LikeButton => _likeButton;
        public ObservableCollection<string> Playlists { get; } = new ObservableCollection<string>();
        public AppBarToggleButton LoopButton { get; private set; }
        public AppBarToggleButton MiniBarToggleButton { get; private set; }
        public ProgressBar MiniSeekBar { get; private set; }
        public List<DeviceInformation> CurrentDevices => _currentDevices ?? new List<DeviceInformation>();
        public TextBlock CurrentTextBlock { get; private set; }
        public AppBarButton OpenButton { get; private set; }
        public AppBarButton FullWindowButton { get; private set; }
        public CommandBar MyCommandBar { get; private set; }
        public TextBox PlaylistBox { get; private set; }

        private Grid _playlistDisabledGrid;
        private Grid _playlistAddGrid;
        private AppBarButton _newPlaylistDoneButton;
        private Button _createNewPlaylistButton;
        private Grid _newPlaylistGrid;

        public ListView PlaylistListView { get; private set; }

        private TextBlock _playlistExists;
        private TextBlock _playlistCharsCountBlock;

        #endregion

        #region Startup
        public FluentMtc()
        {
            DefaultStyleKey = typeof(FluentMtc);
            Loaded += FluentMtc_Loaded;
            //Unloaded += FluentMtc_Unloaded;
        }
        //maybe assing loaded and unloaded to each button/control and assign and unassign click events
        //within those loaded and unloaded events.
        //private void FluentMtc_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    PipButton.Click -= _pipButton_Click;
        //    LoopButton.Checked -= LoopToggleButton_Checked;
        //    LoopButton.Unchecked -= LoopToggleButton_Unchecked;
        //    MiniBarToggleButton.Checked -= _miniBarToggleButton_Checked;
        //    MiniBarToggleButton.Unchecked -= _miniBarToggleButton_Unchecked;
        //    OpenButton.Click -= _openButton_Click;
        //    CloseButton.Click -= CloseButton_Click;

        //    _audioDeviceFlyout.Opened -= AudioDeviceFlyout_Opened;
        //    _audioDeviceComboBox.SelectionChanged -= _audioDeviceComboBox_SelectionChanged;
        //    _progressSlider.PreviewKeyDown -= _progressSlider_PreviewKeyDown;
        //    _volumeSlider.PreviewKeyDown -= _progressSlider_PreviewKeyDown;
        //    ControlsFade.CurrentStateChanging -= ControlsFade_CurrentStateChanging;
        //    Loaded -= FluentMtc_Loaded;
        //    Unloaded -= FluentMtc_Unloaded;
        //}

        private async void FluentMtc_Loaded(object sender, RoutedEventArgs e)
        {
            bool isMiniBarShown = false;
            try
            {
                isMiniBarShown = await ApplicationData.Current.LocalSettings.ReadAsync<bool>(Constants.MiniBarSettingsKey);
            }
            catch
            { }
            finally
            {
                MiniBarToggleButton.IsChecked = isMiniBarShown;
            }
            PreviewKeyDown += FluentMtc_PreviewKeyDown;
        }

        private void FluentMtc_PreviewKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (DeviceHelper.GetDevice() == DeviceHelper.Device.Desktop)
            {
                if (e.Key == Windows.System.VirtualKey.Up || e.Key == Windows.System.VirtualKey.Down ||
                    e.Key == Windows.System.VirtualKey.Left || e.Key == Windows.System.VirtualKey.Right
                    || e.Key == Windows.System.VirtualKey.Space)
                {
                    e.Handled = true;
                }
            }
        }

        protected override void OnApplyTemplate()
        {
            //// Find the custom button and create an event handler for its Click event.
            //_likeButton = (AppBarButton)GetTemplateChild("LikeButton");
            //_likeButton.Click += LikeButton_Clicked;

            MiniSeekBar = GetTemplateChild("MiniSeekBar") as ProgressBar;

            CurrentTextBlock = GetTemplateChild("CurrentTitleBlock") as TextBlock;

            //get volume shortcut key stuff
            VolumeIndicator = GetTemplateChild("VolumeIndicator") as ProgressBar;
            VolumeIndicatorBorder = GetTemplateChild("VolumeIndicatorBorder") as DropShadowPanel;

            //set click for pip mode
            PipButton = (AppBarButton)GetTemplateChild("PipButton");
            if (DeviceHelper.GetDevice() != DeviceHelper.Device.Desktop)
                PipButton.IsEnabled = false;

            PipButton.Click += _pipButton_Click;

            ExtraControls = (Grid)GetTemplateChild("ExtraControls");

            LoopButton = (AppBarToggleButton)GetTemplateChild("LoopToggleButton");
            LoopButton.Checked += LoopToggleButton_Checked;
            LoopButton.Unchecked += LoopToggleButton_Unchecked;

            MiniBarToggleButton = (AppBarToggleButton)GetTemplateChild("MiniBarToggleButton");
            MiniBarToggleButton.Checked += _miniBarToggleButton_Checked;
            MiniBarToggleButton.Unchecked += _miniBarToggleButton_Unchecked;


            //set click for open button
            OpenButton = (AppBarButton)GetTemplateChild("OpenButton");
            OpenButton.Click += _openButton_Click;

            FullWindowButton = (AppBarButton)GetTemplateChild("FullWindowButton");
            FullWindowButton.Click += FullWindowButton_Click;

            CloseButton = (AppBarButton)GetTemplateChild("CloseButton");
            CloseButton.Click += CloseButton_Click;

            AddToPlaylistButton = (AppBarButton)GetTemplateChild("AddToPlaylistButton");

            GoToPlayerPageButton = (AppBarButton)GetTemplateChild("GoToPlayerPageButton");
            GoToPlayerPageButton.Click += GoToPlayerPageButton_Click;
            //Get the audio device selector stuff 
            _audioDeviceFlyout = (Flyout)GetTemplateChild("AudioDeviceFlyout");
            _audioDeviceFlyout.Opened += AudioDeviceFlyout_Opened;
            _audioDeviceComboBox = (ComboBox)GetTemplateChild("AudioDeviceComboBox");
            _audioDeviceComboBox.SelectionChanged += _audioDeviceComboBox_SelectionChanged;

            _playlistFlyout = (Flyout)GetTemplateChild("PlaylistFlyout");
            _playlistFlyout.Opened += _playlistFlyout_Opened;

            VolumeInternalBorder = (Border)GetTemplateChild("VolumeInternalBorder");
            VolumeIcon = (PackIconMaterialDesign)GetTemplateChild("VolumeIcon");

            ControlsFade = (VisualStateGroup)GetTemplateChild("ControlPanelVisibilityStates");
            ControlsFade.CurrentStateChanging += ControlsFade_CurrentStateChanging;

            _progressSlider = (Slider)GetTemplateChild("ProgressSlider");
            _volumeSlider = (Slider)GetTemplateChild("VolumeSlider");

            _progressSlider.PreviewKeyDown += _progressSlider_PreviewKeyDown;
            _volumeSlider.PreviewKeyDown += _progressSlider_PreviewKeyDown;

            MyCommandBar = (CommandBar)GetTemplateChild("MediaControlsCommandBar");

            _playlistDisabledGrid = (Grid)GetTemplateChild("PlaylistDisabledGrid");
            _playlistAddGrid = (Grid)GetTemplateChild("PlaylistAddGrid");

            _newPlaylistDoneButton = (AppBarButton)GetTemplateChild("NewPlaylistDoneButton");
            _newPlaylistDoneButton.Click += _newPlaylistDoneButton_Click;

            _createNewPlaylistButton = (Button)GetTemplateChild("CreateNewPlaylistButton");
            _createNewPlaylistButton.Click += _createNewPlaylistButton_Click;

            _newPlaylistGrid = (Grid)GetTemplateChild("NewPlaylistGrid");

            PlaylistListView = (ListView)GetTemplateChild("PlaylistListView");
            PlaylistListView.ItemsSource = Playlists;
            PlaylistListView.SelectionChanged += PlaylistListView_SelectionChanged;

            _playlistExists = (TextBlock)GetTemplateChild("PlaylistExists");
            _playlistCharsCountBlock = (TextBlock)GetTemplateChild("PlaylistCharsCountBlock");

            PlaylistBox = (TextBox)GetTemplateChild("PlaylistBox");
            PlaylistBox.TextChanged += _playlistBox_TextChanged;

            base.OnApplyTemplate();
        }

        private async void PlaylistListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Playlists.Count > 0)
            {
                foreach (var item in e.AddedItems)
                {
                    await DbHelper.AddToPlaylist(item.ToString(), MediaHelper.MyMediaPlayer.CurrentPlaybackList.CurrentItem.VideoFile.Path);
                }
                foreach (var item in e.RemovedItems)
                {
                    await DbHelper.RemoveFromPlaylist(item.ToString(), MediaHelper.MyMediaPlayer.CurrentPlaybackList.CurrentItem.VideoFile.Path);
                }
            }
        }

        private async void _newPlaylistDoneButton_Click(object sender, RoutedEventArgs e)
        {
            await DbHelper.CreateNewPlaylist(PlaylistBox.Text, MediaHelper.MyMediaPlayer.CurrentPlaybackList.CurrentItem.VideoFile.Path);
            PlaylistDoneClicked.Invoke(this, EventArgs.Empty);
            Playlists.Add(PlaylistBox.Text);
            PlaylistListView.SelectedItems.Add(PlaylistBox.Text);
            RefreshPlaylistsFlyout();
        }

        private async void _playlistBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _playlistCharsCountBlock.Text = PlaylistBox.Text.Length.ToString();
            if (await ValidPlaylistName())
            {
                _newPlaylistDoneButton.IsEnabled = true; _playlistExists.Visibility = Visibility.Collapsed;
            }
            else
            {
                _newPlaylistDoneButton.IsEnabled = false; _playlistExists.Visibility = Visibility.Visible;
            }
        }


        private async Task<bool> ValidPlaylistName()
        {
            if (string.IsNullOrWhiteSpace(PlaylistBox.Text))
                return false;
            else
            {
                return await DbHelper.IsPlaylistNameNew(PlaylistBox.Text);
            }
        }

        private void _createNewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            _createNewPlaylistButton.Visibility = Visibility.Collapsed;
            _newPlaylistGrid.Visibility = Visibility.Visible;
        }

        private async void _playlistFlyout_Opened(object sender, object e)
        {
            if (MediaHelper.MyMediaPlayer?.CurrentPlaybackList?.CurrentItem is null)
            {
                _playlistDisabledGrid.Visibility = Visibility.Visible;
                _playlistAddGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                RefreshPlaylistsFlyout();
                //fill up listview with all playlists and then selected items accordingly.
                Playlists.Clear();
                foreach (var item in await DbHelper.GetPlaylists())
                {
                    Playlists.Add(item);
                }
                foreach (var playlist in await DbHelper.GetPlaylistsForVideo(MediaHelper.MyMediaPlayer.CurrentPlaybackList.CurrentItem.VideoFile.Path))
                {
                    PlaylistListView.SelectedItems.Add(playlist);
                }
            }
        }

        private void RefreshPlaylistsFlyout()
        {
            //refresh everything
            _playlistDisabledGrid.Visibility = Visibility.Collapsed;
            _playlistAddGrid.Visibility = Visibility.Visible;
            _createNewPlaylistButton.Visibility = Visibility.Visible;
            _newPlaylistGrid.Visibility = Visibility.Collapsed;

            _playlistCharsCountBlock.Text = 0.ToString();
            PlaylistBox.Text = "";
            _newPlaylistDoneButton.IsEnabled = false; _playlistExists.Visibility = Visibility.Visible;
        }

        private void FullWindowButton_Click(object sender, RoutedEventArgs e) => FullWindowButtonClicked?.Invoke(this, EventArgs.Empty);

        private void ControlsFade_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            bool fadein = false;
            if (e.NewState.Name == "ControlPanelFadeIn")
                fadein = true;

            ControlFadeChanged?.Invoke(this, new ControlFadeChangedEventArgs { Appeared = fadein });
        }

        public void VolumeIndicatorAppear(bool up)
        {
            if (up)
                VolumeIcon.Kind = PackIconMaterialDesignKind.VolumeUp;
            else
                VolumeIcon.Kind = PackIconMaterialDesignKind.VolumeDown;

            VolumeIndicatorBorder.Visibility = Visibility.Visible;
            var animation = new OpacityAnimation() { To = 1, Duration = Constants.VolumeAppearTime };
            animation.StartAnimation(VolumeIndicatorBorder);
        }
        public async void VolumeIndicatorDisAppear()
        {
            var animation2 = new OpacityAnimation() { To = 0, Duration = Constants.VolumeDisappearTime };
            await Task.Delay(1500);
            animation2.StartAnimation(VolumeIndicatorBorder);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => CloseButtonClicked?.Invoke(this, EventArgs.Empty);
        private void GoToPlayerPageButton_Click(object sender, RoutedEventArgs e) => GoToPlayerPageButtonClicked?.Invoke(this, EventArgs.Empty);
        private void _progressSlider_PreviewKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (DeviceHelper.GetDevice() == DeviceHelper.Device.Desktop)
            {
                if (e.Key == Windows.System.VirtualKey.Up || e.Key == Windows.System.VirtualKey.Down ||
                    e.Key == Windows.System.VirtualKey.Left || e.Key == Windows.System.VirtualKey.Right)
                {
                    e.Handled = true;
                }
            }
        }

        private void _openButton_Click(object sender, RoutedEventArgs e) => OpenButtonClicked?.Invoke(this, EventArgs.Empty);


        private void _pipButton_Click(object sender, RoutedEventArgs e) => PipToggled?.Invoke(this, EventArgs.Empty);


        private async void _miniBarToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            MiniSeekBar.Visibility = Visibility.Collapsed;
            await ApplicationData.Current.LocalSettings.SaveAsync(Constants.MiniBarSettingsKey, false);
        }

        private async void _miniBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            MiniSeekBar.Visibility = Visibility.Visible;
            await ApplicationData.Current.LocalSettings.SaveAsync(Constants.MiniBarSettingsKey, true);
        }

        private void LoopToggleButton_Unchecked(object sender, RoutedEventArgs e) => LoopingToggled?.Invoke(this, new LoopingToggledEventArgs { IsChecked = (bool)((AppBarToggleButton)sender).IsChecked });

        private void LoopToggleButton_Checked(object sender, RoutedEventArgs e) => LoopingToggled?.Invoke(this, new LoopingToggledEventArgs { IsChecked = (bool)((AppBarToggleButton)sender).IsChecked });

        private void _audioDeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_audioDeviceComboBox.SelectedIndex != -1)
            {
                AudioDeviceChanged?.Invoke(this,
                    new AudioDeviceChangedEventArgs
                    {
                        SelectedIndex = _audioDeviceComboBox.SelectedIndex,
                        AudioDevice = CurrentDevices[_audioDeviceComboBox.SelectedIndex]
                    });
            }
        }

        private async void AudioDeviceFlyout_Opened(object sender, object e)
        {
            try
            {
                _audioDeviceComboBox.Items.Clear();
                _currentDevices = new List<DeviceInformation>();
                string audioSelector = MediaDevice.GetAudioRenderSelector();
                var defaultDeviceId = MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default);
                var outputDevices = await DeviceInformation.FindAllAsync(audioSelector);
                DeviceInformation defaultDevice = null;
                foreach (var device in outputDevices)
                {
                    var deviceItem = new ComboBoxItem();
                    deviceItem.Content = device.Name;
                    deviceItem.Tag = device;
                    _currentDevices.Add(device);
                    if (device.Id == defaultDeviceId)
                    { defaultDevice = device; }
                    _audioDeviceComboBox.Items.Add(deviceItem);
                }
                if (defaultDevice != null)
                {
                    _audioDeviceComboBox.SelectedIndex = CurrentDevices.IndexOf(defaultDevice);
                }
            }
            catch { }
        }


        #endregion

        #region PrivateHandlers


        //private void LikeButton_Clicked(object sender, RoutedEventArgs e)
        //{
        //    // Raise an event on the custom control when 'like' is clicked.
        //    var likeButton = (AppBarButton)sender;
        //    var icon = (FontIcon)(likeButton.Icon);
        //    if (icon.Glyph == "A")
        //    {
        //        LikeToggled?.Invoke(this, new LikeToggledEventArgs(LikeToggledValue.Liked));
        //    }
        //    else
        //    {
        //        LikeToggled?.Invoke(this, new LikeToggledEventArgs(LikeToggledValue.Unliked));
        //    }
        //}
        #endregion


    }

    public class ControlFadeChangedEventArgs
    {
        public bool Appeared { get; set; }
    }
}
