using Fluent_Video_Player.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Fluent_Video_Player.Views;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel
    {
        get;
    }

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();
    }
}
