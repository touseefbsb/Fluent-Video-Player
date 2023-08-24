using System.Reflection;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Fluent_Video_Player.Contracts.Services;
using Fluent_Video_Player.Helpers;

using Microsoft.UI.Xaml;

using Windows.ApplicationModel;

namespace Fluent_Video_Player.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    #region Services
    private readonly IThemeSelectorService _themeSelectorService;
    #endregion Services

    #region ObservableFields
    [ObservableProperty] private ElementTheme _elementTheme;
    [ObservableProperty] private string _versionDescription;
    #endregion ObservableFields

    #region Ctor
    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
    }
    #endregion Ctor

    #region Methods
    [RelayCommand]
    private async Task SwitchThemeAsync(ElementTheme param)
    {
        if (ElementTheme != param)
        {
            ElementTheme = param;
            await _themeSelectorService.SetThemeAsync(param);
        }
    }
    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
    #endregion Methods
}
