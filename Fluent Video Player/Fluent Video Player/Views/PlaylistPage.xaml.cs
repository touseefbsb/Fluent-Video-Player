using System;
using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using Fluent_Video_Player.Services;
using Fluent_Video_Player.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fluent_Video_Player.Views
{
    public sealed partial class PlaylistPage : Page
    {
        public PlaylistViewModel ViewModel { get; } = new PlaylistViewModel();

        public PlaylistPage() { InitializeComponent(); ViewModel.FileNotFoundInAppNotification = FileNotFoundInAppNotification; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.ShellVM = DataContext as ShellViewModel;
            ViewModel.PlaylistTitle = e.Parameter as string;
            MyFluentGridView.SearchBox.TextChanged += AutoSuggestBox_TextChanged;
            MyFluentGridView.MyGridView.ItemClick += MyGridView_ItemClick;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MyFluentGridView.SearchBox.TextChanged -= AutoSuggestBox_TextChanged;
            MyFluentGridView.MyGridView.ItemClick -= MyGridView_ItemClick;
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) =>
            ApplyFilter();

        private void ApplyFilter()
        {
            ViewModel.Source.Filter = x => true;
            if (string.IsNullOrWhiteSpace(MyFluentGridView.SearchBox.Text))
                ViewModel.Source.Filter = x => true;
            else
            {
                ViewModel.Source.Filter = x =>
                ((Video)x).Title.Contains(MyFluentGridView.SearchBox.Text, StringComparison.OrdinalIgnoreCase);
            }
        }

        private async void MyGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            if (item is Video video)
            {
                try
                {
                    await MediaHelper.MyMediaPlayer.SetNewSourceWithVideoCollection((Video)e.ClickedItem, ViewModel.SourcePrivate, ViewModel.PlaylistTitle);
                    NavigationService.Navigate(typeof(PlayerPage));
                }
                catch(System.IO.FileNotFoundException)
                {
                    await ViewModel.FillPlaylist();
                }                
            }
        }

        private async void HistoryTemplate_VideoRemovedFromPlaylist(object sender, DataTemplates.VideoRemovedFromPlaylistEventArgs e)
        {
            ViewModel.SourcePrivate.Remove(e.MyVideo);
            await Database.DbHelper.RemoveFromPlaylist(ViewModel.PlaylistTitle, e.MyVideo.MyVideoFile.Path);
        }

    }
}
