using Fluent_Video_Player.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Fluent_Video_Player.Views;

public sealed partial class PlaylistPage : Page
{
    public PlaylistViewModel ViewModel
    {
        get;
    }

    public PlaylistPage()
    {
        ViewModel = App.GetService<PlaylistViewModel>();
        InitializeComponent();
    }
}
