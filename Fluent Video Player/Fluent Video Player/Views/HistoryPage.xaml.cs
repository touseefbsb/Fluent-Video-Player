using Fluent_Video_Player.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Fluent_Video_Player.Views;

public sealed partial class HistoryPage : Page
{
    public HistoryViewModel ViewModel { get; }

    public HistoryPage()
    {
        ViewModel = App.GetService<HistoryViewModel>();
        ViewModel.FileNotFoundInAppNotification = FileNotFoundInAppNotification;
        InitializeComponent();
    }
}
