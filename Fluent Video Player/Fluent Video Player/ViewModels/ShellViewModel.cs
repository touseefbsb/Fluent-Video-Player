using CommunityToolkit.Mvvm.ComponentModel;

using Fluent_Video_Player.Contracts.Services;
using Fluent_Video_Player.Views;

using Microsoft.UI.Xaml.Navigation;

namespace Fluent_Video_Player.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty] private bool isBackEnabled;

    [ObservableProperty] private object? selected;

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}
