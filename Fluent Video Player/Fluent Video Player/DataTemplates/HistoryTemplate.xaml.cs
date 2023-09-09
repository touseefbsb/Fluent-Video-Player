using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using System;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Fluent_Video_Player.DataTemplates
{
    public sealed partial class HistoryTemplate : UserControl
    {
        public HistoryTemplate() => InitializeComponent();

        public Video MyVideo
        {
            get => (Video)GetValue(MyVideoProperty);
            set => SetValue(MyVideoProperty, value);
        }

        public static readonly DependencyProperty MyVideoProperty =
            DependencyProperty.Register("MyVideo", typeof(Video), typeof(HistoryTemplate), new PropertyMetadata(null));
        
        public bool IsPlaylist
        {
            get { return (bool)GetValue(IsPlaylistProperty); }
            set { SetValue(IsPlaylistProperty, value); }
        }
        
        public static readonly DependencyProperty IsPlaylistProperty =
            DependencyProperty.Register("IsPlaylist", typeof(bool), typeof(HistoryTemplate), new PropertyMetadata(null));


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CheckView();
            SettingsStorageExtensions.OnCollectionViewSelected += SettingsStorageExtensions_OnCollectionViewSelected;
        }
        private void SettingsStorageExtensions_OnCollectionViewSelected(object sender, System.EventArgs e) => CheckView();
        private void UserControl_Unloaded(object sender, RoutedEventArgs e) => SettingsStorageExtensions.OnCollectionViewSelected -= SettingsStorageExtensions_OnCollectionViewSelected;
        private void CheckView()
        {
            switch (ApplicationData.Current.LocalSettings.ReadCurrentCollectionView())
            {
                case CurrentCollectionView.GridView:
                    TurnToGridView();
                    break;
                case CurrentCollectionView.ListView:
                    TurnToListView();
                    break;
            }
        }
        private void TurnToGridView()
        {
            GridVieww.Visibility = Visibility.Visible;
            ListVieww.Visibility = Visibility.Collapsed;
        }
        private void TurnToListView()
        {
            GridVieww.Visibility = Visibility.Collapsed;
            ListVieww.Visibility = Visibility.Visible;
        }

        private ICommand _historyDeleteCommand;
        public ICommand HistoryDeleteCommand => _historyDeleteCommand ?? (_historyDeleteCommand = new RelayCommand(OnHistoryDelete));

        private void OnHistoryDelete()
        {
            if (IsPlaylist)
            {
                VideoRemovedFromPlaylist?.Invoke(this, new VideoRemovedFromPlaylistEventArgs { MyVideo = MyVideo });
            }
            else
            {
                HistoryItemDeleted?.Invoke(this, new HistoryItemDeletedEventArgs { MyVideo = MyVideo });                
            }
        }
        public event EventHandler<VideoRemovedFromPlaylistEventArgs> VideoRemovedFromPlaylist;
        public event EventHandler<HistoryItemDeletedEventArgs> HistoryItemDeleted;
    }

    public class VideoRemovedFromPlaylistEventArgs
    {
        public Video MyVideo { get; set; }
    }
    public class HistoryItemDeletedEventArgs
    {
        public Video MyVideo { get; set; }
    }
}
