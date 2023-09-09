using Fluent_Video_Player.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Fluent_Video_Player.Views
{
    public sealed partial class HomePage : Page
    {
        public HomeViewModel ViewModel { get; } = new();
        public HomePage() => InitializeComponent();
    }
}
