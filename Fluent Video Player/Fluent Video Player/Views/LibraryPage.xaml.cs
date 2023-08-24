using Fluent_Video_Player.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Fluent_Video_Player.Views;

public sealed partial class LibraryPage : Page
{
    public LibraryViewModel ViewModel
    {
        get;
    }

    public LibraryPage()
    {
        ViewModel = App.GetService<LibraryViewModel>();
        InitializeComponent();
    }
}
