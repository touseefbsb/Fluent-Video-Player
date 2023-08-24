using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.Input;
using Fluent_Video_Player.Core.Enums;
using Fluent_Video_Player.EventArgs;
using Fluent_Video_Player.Extensions;
using Fluent_Video_Player.Models;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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
            get => (bool)GetValue(IsPlaylistProperty);
            set => SetValue(IsPlaylistProperty, value);
        }

        public static readonly DependencyProperty IsPlaylistProperty =
            DependencyProperty.Register("IsPlaylist", typeof(bool), typeof(HistoryTemplate), new PropertyMetadata(null));

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CheckView();
            SettingsStorageExtensions.OnCollectionViewSelected += SettingsStorageExtensions_OnCollectionViewSelected;
        }
        private void SettingsStorageExtensions_OnCollectionViewSelected(object? sender, System.EventArgs e) => CheckView();
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
        [RelayCommand]
        private void HistoryDelete()
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
}
