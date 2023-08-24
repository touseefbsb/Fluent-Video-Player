using Fluent_Video_Player.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Fluent_Video_Player.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }
}
