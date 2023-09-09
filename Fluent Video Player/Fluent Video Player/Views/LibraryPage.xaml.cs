using System;
using System.Threading.Tasks;
using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using Fluent_Video_Player.Services;
using Fluent_Video_Player.ViewModels;
using MahApps.Metro.IconPacks;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Fluent_Video_Player.Views
{
    public sealed partial class LibraryPage : Page
    {
        public LibraryViewModel ViewModel { get; } = new LibraryViewModel();
        public LibraryPage() => InitializeComponent();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("FolderConnectedAnimation");
            if (!(anim is null))
                anim.TryStart(FolderIconTitle);

            MyFluentGridView.MyGridView.ItemClick += MyGridView_ItemClick;
            MyFluentGridView.SearchBox.TextChanged += AutoSuggestBox_TextChanged;
            ShowFoldersListView.SelectedIndex = 0;

            ViewModel.Initialize(e.Parameter as StorageFolder);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MyFluentGridView.MyGridView.ItemClick -= MyGridView_ItemClick;
            MyFluentGridView.SearchBox.TextChanged -= AutoSuggestBox_TextChanged;
            ViewModel.Dispose();
        }

        #region FilterAndSort
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) =>
            ApplyFilter();

        private void ApplyFilter()
        {
            ViewModel.Source.Filter = x => true;
            if (string.IsNullOrWhiteSpace(MyFluentGridView.SearchBox.Text) && ShowFoldersListView.SelectedIndex < 1)
                ViewModel.Source.Filter = x => true;// both filters are empty.            
            else if ((!string.IsNullOrWhiteSpace(MyFluentGridView.SearchBox.Text)) && ShowFoldersListView.SelectedIndex > 0)
            {
                //search filter will apply and also the showfolder filter will apply because both are valid here.
                if (ShowFoldersListView.SelectedIndex == 1)// Show Folder + text Filter                
                    ViewModel.Source.Filter = x => (
                    (((IVideoFolder)x).Title.Contains(MyFluentGridView.SearchBox.Text, StringComparison.OrdinalIgnoreCase)) &&
                    (((IVideoFolder)x) is Folder));
                else //Show Video + text filter                
                    ViewModel.Source.Filter = x => (
                    (((IVideoFolder)x).Title.Contains(MyFluentGridView.SearchBox.Text, StringComparison.OrdinalIgnoreCase)) &&
                    (((IVideoFolder)x) is Video));
            }
            //filter only with text because only text filter is valid here.
            else if ((!string.IsNullOrWhiteSpace(MyFluentGridView.SearchBox.Text)) && ShowFoldersListView.SelectedIndex < 1)
                ViewModel.Source.Filter = x =>
                ((IVideoFolder)x).Title.Contains(MyFluentGridView.SearchBox.Text, StringComparison.OrdinalIgnoreCase);
            else// filter only with folder/video and not with text here.
            {
                if (ShowFoldersListView.SelectedIndex == 1)// Show only Folder filter                
                    ViewModel.Source.Filter = x => ((IVideoFolder)x) is Folder;
                else //Show only Video filter.                
                    ViewModel.Source.Filter = x => ((IVideoFolder)x) is Video;
            }
        }

        //private void SortChecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    //sort the names ascending.
        //    ViewModel.Source.SortDescriptions.Clear();
        //    ViewModel.Source.SortDescriptions.Add(new SortDescription("Title", SortDirection.Ascending));
        //}

        //private void SortUnChecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    //sort the names descending
        //    ViewModel.Source.SortDescriptions.Clear();
        //}
        //private void SortToggleButton_Indeterminate(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    ViewModel.Source.SortDescriptions.Clear();
        //    ViewModel.Source.SortDescriptions.Add(new SortDescription("Title", SortDirection.Descending));
        //}

        #endregion

        private async void MyGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            try
            {
                if (item is Folder folder)
                {
                    var folderIcon = (sender as AdaptiveGridView).ContainerFromItem(item).FindDescendant<PackIconMaterialDesign>() as UIElement;
                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("FolderConnectedAnimation", folderIcon);
                    NavigationService.Navigate(typeof(LibraryPage), folder?.MyStorageFolder, new SuppressNavigationTransitionInfo(), navigateToSamePage: true);
                }
                else if (item is Video video)
                {
                    await MediaHelper.MyMediaPlayer.SetNewSourceWithVideoCollection((Video)e.ClickedItem, ViewModel.SourcePrivate, ViewModel.MainFolder.DisplayName);
                    NavigationService.Navigate(typeof(PlayerPage));
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                if (ViewModel.MainFolder is null)
                {
                    await ViewModel.Fill(KnownFolders.VideosLibrary);
                }
                else
                {
                    await ViewModel.Fill(ViewModel.MainFolder);
                }
                FileNotFoundInAppNotification.Show();
                await Task.Delay(6000);
                FileNotFoundInAppNotification.Dismiss();
            }
        }
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();
    }
}
