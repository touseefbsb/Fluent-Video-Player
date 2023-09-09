
using Fluent_Video_Player.ViewModels;
using static Fluent_Video_Player.Helpers.MediaHelper;
using Windows.UI.Xaml.Controls;
using Fluent_Video_Player.Services;
using Windows.UI.Xaml.Media.Animation;
using Fluent_Video_Player.Helpers;

namespace Fluent_Video_Player.Views
{
    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; } = new ShellViewModel();

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.Initialize(this, shellFrame, winUiNavigationView, KeyboardAccelerators,
                Minimpe, MiniFluentmtc, MiniPlayerGrid, DisplayModeChangeButton, RatingInAppNotification);
            
        }

        private void Page_PreviewKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (DeviceHelper.GetDevice() == DeviceHelper.Device.Desktop && !(e.OriginalSource is TextBox))
            {
                if (e.Key == Windows.System.VirtualKey.Up || e.Key == Windows.System.VirtualKey.Down ||
                    e.Key == Windows.System.VirtualKey.Left || e.Key == Windows.System.VirtualKey.Right ||
                    e.Key == Windows.System.VirtualKey.Space) { e.Handled = true; }
            }
        }
        private void Fluentmtc_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //deal with like icon
            //LikedIconUpdate();
            //adjust the volume for start.

            MiniFluentmtc.VolumeIndicator.Value = MyMediaPlayer.MP.Volume;
            MiniFluentmtc.LoopButton.IsChecked = MyMediaPlayer.CurrentPlaybackList.AutoRepeatEnabled;
            if (MyMediaPlayer.CurrentPlaybackList.Items.Count > 0 && MyMediaPlayer.CurrentPlaybackList.CurrentItem != default(object))
            {
                MiniFluentmtc.CurrentTextBlock.Text = MyMediaPlayer?.CurrentPlaybackList?.CurrentItem?.GetDisplayProperties()?.VideoProperties?.Title;
                ToolTipService.SetToolTip(MiniFluentmtc.CurrentTextBlock, MiniFluentmtc.CurrentTextBlock.Text);
            }
            //Fluentmtc.ExtraControls.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            MiniFluentmtc.MyCommandBar?.PrimaryCommands.Remove(MiniFluentmtc.PipButton);
            MiniFluentmtc.OpenButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            MiniFluentmtc.AddToPlaylistButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            MiniFluentmtc.CloseButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            MiniFluentmtc.GoToPlayerPageButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            MiniFluentmtc.VolumeInternalBorder.Height = 42.0;
            MiniFluentmtc.VolumeInternalBorder.Width = 50.0;
            MiniFluentmtc.VolumeIcon.Width = 20.0;
            MiniFluentmtc.VolumeIcon.Height = 20.0;
            MiniFluentmtc.VolumeIndicator.Height = 4.0;
            MiniFluentmtc.VolumeIndicatorBorder.BlurRadius = 10.0;
        }

        private void Minimpe_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("MiniConnectedAnimation", MiniPlayerGrid);
            NavigationService.Navigate<PlayerPage>(infoOverride: new SuppressNavigationTransitionInfo());
        }
        
    }
}
