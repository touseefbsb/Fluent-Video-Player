
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fluent_Video_Player.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static Fluent_Video_Player.Helpers.Constants;
namespace Fluent_Video_Player.ViewModels
{
    public partial class PlayerViewModel : ObservableObject
    {
        public object SecondGrid { get; set; }
        public ObservableCollection<FluentPlaybackItem> Source { get; } = new ObservableCollection<FluentPlaybackItem>();
        // The poster image is displayed until the video is started
        private const string DefaultPoster = "ms-appx:///Assets/SplashScreen.scale-200.png";

        [ObservableProperty] private ImageSource posterSource;
        [ObservableProperty] private string playlistTitle;
        [ObservableProperty] private bool secondGridVisibility;

        #region mpeResponsiveProps
        [ObservableProperty] private bool mpeAlignLeftWithPanel;
        [ObservableProperty] private bool mpeAlignTopWithPanel;
        [ObservableProperty] private bool mpeAlignRightWithPanel;
        [ObservableProperty] private bool mpeAlignBottomWithPanel;
        [ObservableProperty] private object mpeAbove;
        [ObservableProperty] private object mpeLeftOf;
        #endregion mpeResponsiveProps        

        #region SecondGridResponsiveProps
        [ObservableProperty] private bool secondGridAlignLeftWithPanel;
        [ObservableProperty] private bool secondGridAlignTopWithPanel;
        [ObservableProperty] private bool secondGridAlignRightWithPanel;
        [ObservableProperty] private bool secondGridAlignBottomWithPanel;
        [ObservableProperty] private double secondGridWidth;
        [ObservableProperty] private double secondGridHeight;
        [ObservableProperty] private double playListDesiredWidth;
        [ObservableProperty] private double playListItemHeight;
        [ObservableProperty] private bool oneRowMode;
        [ObservableProperty] private double playlistHeaderHeight;
        #endregion SecondGridResponsiveProps

        [RelayCommand] private void StateChanged(VisualStateChangedEventArgs args) => GoToState(args.NewState.Name);

        private void InitializeState(double windowWith)
        {
            if (windowWith < WideStateMinWindowWidth)
            {
                GoToState(NarrowStateName);
            }
            else if (windowWith < PanoramicStateMinWindowWidth)
            {
                GoToState(WideStateName);
            }
            else
            {
                GoToState(PanoramicStateName);
            }
        }
        private void GoToState(string stateName)
        {
            switch (stateName)
            {
                case PanoramicStateName: SetDefaultView(); break;
                case WideStateName: SetDefaultView(); break;
                case NarrowStateName: SetNarrowView(); break;
                default: SetDefaultView(); break;
            }
        }
        private void SetNarrowView()
        {
            MpeAlignLeftWithPanel = true;
            MpeAlignTopWithPanel = true;
            MpeAlignRightWithPanel = true;
            MpeAlignBottomWithPanel = false;
            MpeAbove = SecondGrid;
            MpeLeftOf = null;

            SecondGridAlignLeftWithPanel = true;
            SecondGridAlignTopWithPanel = false;
            SecondGridAlignRightWithPanel = true;
            SecondGridAlignBottomWithPanel = true;

            SecondGridWidth = double.NaN;
            SecondGridHeight = Constants.SecondGridHeight;
            PlayListDesiredWidth = PlayGridDesiredWidth;
            PlayListItemHeight = PlayGridItemHeight;
            OneRowMode = true;
            PlaylistHeaderHeight = double.NaN;
        }
        private void SetDefaultView()
        {
            MpeAlignLeftWithPanel = true;
            MpeAlignTopWithPanel = true;
            MpeAlignRightWithPanel = false;
            MpeAlignBottomWithPanel = true;
            MpeAbove = null;
            MpeLeftOf = SecondGrid;

            SecondGridAlignLeftWithPanel = false;
            SecondGridAlignTopWithPanel = true;
            SecondGridAlignRightWithPanel = true;
            SecondGridAlignBottomWithPanel = true;

            SecondGridWidth = Constants.SecondGridWidth;
            SecondGridHeight = double.NaN;
            PlayListDesiredWidth = Constants.PlayListDesiredWidth;
            PlayListItemHeight = Constants.PlayListItemHeight;
            OneRowMode = false;
            PlaylistHeaderHeight = Constants.PlaylistHeaderHeight;
        }
        public void Initialize()
        {
            PlaylistTitle = MediaHelper.MyMediaPlayer.PlaylistTitle;
            InitializeState(Window.Current.Bounds.Width);
            PosterSource = new BitmapImage
            {
                UriSource = new System.Uri(DefaultPoster)
            };
            if (MediaHelper.MyMediaPlayer?.CurrentPlaybackList?.Items?.Count > 1)
            {
                Source.Clear();
                foreach (var item in MediaHelper.MyMediaPlayer.CurrentPlaybackList.Items)
                {
                    Source.Add(item);
                }
                if (Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().ViewMode == Windows.UI.ViewManagement.ApplicationViewMode.Default)
                {
                    SecondGridVisibility = true;
                }
            }
            else
            {
                SecondGridVisibility = false;
            }
        }
    }
}
