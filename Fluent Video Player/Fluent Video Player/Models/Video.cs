using CommunityToolkit.Mvvm.ComponentModel;
using Fluent_Video_Player.Enums;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace Fluent_Video_Player.Models
{
    /// <summary>
    /// the video object which will be used throughout the app to deal with video file
    /// </summary>
    public partial class Video : ObservableRecipient, IVideoFolder
    {
        #region ObservableFields
        [ObservableProperty] private ImageSource _thumbnail;
        [ObservableProperty] private string _duration;
        [ObservableProperty] private bool _deleteButtonEnabled;
        [ObservableProperty] private string _historyToken;
        #endregion ObservableFields

        #region Props
        public Video THIS => this;
        public StorageFile MyVideoFile { get; set; }
        public string Title => MyVideoFile.DisplayName;
        #endregion Props

        #region Ctor
        public Video(StorageFile file)
        {
            PropertyChanged += Video_PropertyChanged;
            MyVideoFile = file;
            Thumbnail = new BitmapImage(new Uri("ms-appx:///Assets/SplashScreen.scale-200.png"));
        }
        #endregion Ctor

        #region Methods
        private void Video_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(HistoryToken) && !string.IsNullOrWhiteSpace(HistoryToken))
            {
                DeleteButtonEnabled = true;
            }
        }
        #endregion Methods
    }
}
