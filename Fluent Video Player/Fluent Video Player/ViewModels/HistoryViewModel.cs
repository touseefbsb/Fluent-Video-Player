using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Fluent_Video_Player.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;

namespace Fluent_Video_Player.ViewModels
{
    public partial class HistoryViewModel : HomeHistoryViewModel
    {
        public string HistoryTitle;
        public AdvancedCollectionView HistorySource { get; set; }
        public HistoryViewModel()
        {
            HistoryTitle = "History".GetLocalized();
            HistorySource = new AdvancedCollectionView(HistorySourcePrivate, true);
        }
        [RelayCommand] private async Task LoadedAsync() => await FillHistoryAsync();

        [RelayCommand]
        private async Task ClearHistoryAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "ConfirmationTitle".GetLocalized(),
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "ContentDialogPrimaryText".GetLocalized(),
                SecondaryButtonText = "ContentDialogSecondaryText".GetLocalized(),
                Content = "ConfirmClearHistoryText".GetLocalized()
            };
            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.Primary:
                    StorageApplicationPermissions.FutureAccessList.Clear();
                    HistorySourcePrivate.Clear();
                    break;
                case ContentDialogResult.Secondary:
                case ContentDialogResult.None:
                    break;
            }
        }
    }
}
