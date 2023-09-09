using System;
using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using Fluent_Video_Player.Services;
using Fluent_Video_Player.ViewModels;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fluent_Video_Player.Views
{
    public sealed partial class HistoryPage : Page
    {
        public HistoryViewModel ViewModel { get; } = new HistoryViewModel();
        public HistoryPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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
            ViewModel.HistorySource.Filter = _ => true;
            ViewModel.HistorySource.Filter = string.IsNullOrWhiteSpace(MyFluentGridView.SearchBox.Text)
                ? (_ => true)
                : (x =>
                ((Video)x).Title.Contains(MyFluentGridView.SearchBox.Text, StringComparison.OrdinalIgnoreCase));
        }

        private async void MyGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            if (item is Video video)
            {
                try
                {
                    await MediaHelper.MyMediaPlayer.SetNewSourceWithVideoCollection((Video)e.ClickedItem, ViewModel.HistorySourcePrivate, "History".GetLocalized());
                    NavigationService.Navigate(typeof(PlayerPage));
                }
                catch (System.IO.FileNotFoundException)
                {
                    await ViewModel.FillHistoryAsync();
                }
            }
        }

        private void HistoryTemplate_HistoryItemDeleted(object sender, DataTemplates.HistoryItemDeletedEventArgs e)
        {
            StorageApplicationPermissions.FutureAccessList.Remove(e.MyVideo.HistoryToken);
            ViewModel.HistorySourcePrivate.Remove(e.MyVideo);
        }
    }
}
