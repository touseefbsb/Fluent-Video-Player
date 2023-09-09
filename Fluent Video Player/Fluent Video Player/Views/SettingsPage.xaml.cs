using System;
using Fluent_Video_Player.Models;
using Fluent_Video_Player.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fluent_Video_Player.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

        public SettingsPage()
        {
            InitializeComponent();
            ViewModel.ShellVm = DataContext as ShellViewModel;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.Initialize();
            MyFluentGridView.SearchBox.TextChanged += AutoSuggestBox_TextChanged;
            MyShortCutGridView.SearchBox.TextChanged += ShortCut_TextChanged;
        }

        private void ShortCut_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ViewModel.KeyboardShortCuts.Filter = x => true;
            if (string.IsNullOrWhiteSpace(MyShortCutGridView.SearchBox.Text))
                ViewModel.KeyboardShortCuts.Filter = x => true;
            else
            {
                ViewModel.KeyboardShortCuts.Filter = x =>
                ((KeyboardShortCut)x).Description.Contains(MyShortCutGridView.SearchBox.Text, StringComparison.OrdinalIgnoreCase);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MyFluentGridView.SearchBox.TextChanged -= AutoSuggestBox_TextChanged;
            MyShortCutGridView.SearchBox.TextChanged -= ShortCut_TextChanged;
            ViewModel.Dispose();
        }
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => ApplyFilter();

        private void ApplyFilter()
        {
            ViewModel.LibraryFolders.Filter = x => true;
            if (string.IsNullOrWhiteSpace(MyFluentGridView.SearchBox.Text))
                ViewModel.LibraryFolders.Filter = x => true;
            else
            {
                ViewModel.LibraryFolders.Filter = x =>
                ((Folder)x).Title.Contains(MyFluentGridView.SearchBox.Text, StringComparison.OrdinalIgnoreCase);
            }
        }

        private async void VideoFolderTemplate_LibraryFolderRemoved(object sender, DataTemplates.LibraryFolderRemovedEventArgs e)
        {
            try
            {
                var removed = await ViewModel.VideosLibrary.RequestRemoveFolderAsync(e.MyFolder.MyStorageFolder);
            }
            catch { }
        }
        
    }
}
