using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Fluent_Video_Player.Models
{
    /// <summary>
    /// the video object which will be used throughout the app to deal with video file
    /// </summary>
    public partial class Video : ObservableObject, IVideoFolder
    {
        public Video(StorageFile file)
        {
            PropertyChanged += Video_PropertyChanged;
            MyVideoFile = file;
            Thumbnail = new BitmapImage(new Uri("ms-appx:///Assets/SplashScreen.scale-200.png"));
        }

        private void Video_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(HistoryToken) && !string.IsNullOrWhiteSpace(HistoryToken))
            {
                DeleteButtonEnabled = true;
            }
        }

        public StorageFile MyVideoFile { get; set; }
        public string Title => MyVideoFile?.DisplayName;

        [ObservableProperty] private ImageSource thumbnail;
        [ObservableProperty] private string duration;
        [ObservableProperty] private bool deleteButtonEnabled;
        [ObservableProperty] private string historyToken;

        public Video THIS => this;
    }
}
