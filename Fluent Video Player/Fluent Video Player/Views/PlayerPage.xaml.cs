using Fluent_Video_Player.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Fluent_Video_Player.Views;

public sealed partial class PlayerPage : Page
{
    public PlayerViewModel ViewModel
    {
        get;
    }

    public PlayerPage()
    {
        ViewModel = App.GetService<PlayerViewModel>();
        InitializeComponent();
    }
}
