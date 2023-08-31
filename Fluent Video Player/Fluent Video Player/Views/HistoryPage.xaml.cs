using CommunityToolkit.WinUI;
using Fluent_Video_Player.Contracts.Services;
using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Models;
using Fluent_Video_Player.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Fluent_Video_Player.Views;

public sealed partial class HistoryPage : Page
{
    #region Services
    private readonly INavigationService _navigationService;
    #endregion Services

    public HistoryViewModel ViewModel { get; }

    public HistoryPage(INavigationService navigationService)
    {
        ViewModel = App.GetService<HistoryViewModel>();
        _navigationService = navigationService;
        ViewModel.FileNotFoundInAppNotification = FileNotFoundInAppNotification;
        InitializeComponent();
    }
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
        ViewModel.ApplyFilter(MyFluentGridView.SearchBox.Text);

    private async void MyGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        var item = e.ClickedItem;
        if (item is Video)
        {
            try
            {
                await MediaHelper.MyMediaPlayer.SetNewSourceAsync((Video)e.ClickedItem, ViewModel.SourcePrivate, "History".GetLocalized());
                _navigationService.NavigateTo(nameof(PlayerPage));
            }
            catch (System.IO.FileNotFoundException)
            {
                await ViewModel.FillHistoryAsync();
            }
        }
    }

    private void HistoryTemplate_HistoryItemDeleted(object sender, EventArgs.HistoryItemDeletedEventArgs e) =>
        ViewModel.HistoryItemDeleted(e);
}
