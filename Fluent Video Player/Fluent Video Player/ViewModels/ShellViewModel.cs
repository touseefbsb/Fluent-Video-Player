using CommunityToolkit.Mvvm.ComponentModel;

using Fluent_Video_Player.Contracts.Services;
using Fluent_Video_Player.Views;

using Microsoft.UI.Xaml.Navigation;

namespace Fluent_Video_Player.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    #region Services
    public INavigationService NavigationService { get; }
    public INavigationViewService NavigationViewService { get; }
    #endregion Services

    #region ObservableFields
    [ObservableProperty] private bool isBackEnabled;
    [ObservableProperty] private object? selected;
    #endregion ObservableFields

    #region Ctor
    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
    }
    #endregion Ctor

    #region Methods
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
    #endregion Methods
}
