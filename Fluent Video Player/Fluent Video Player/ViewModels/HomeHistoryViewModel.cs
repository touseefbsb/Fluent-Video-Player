using CommunityToolkit.Mvvm.ComponentModel;
using Fluent_Video_Player.Extensions;
using Fluent_Video_Player.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Fluent_Video_Player.ViewModels
{
    public partial class HomeHistoryViewModel : ObservableObject
    {
        [ObservableProperty] private bool historyLoading;
        public ObservableCollection<Video> HistorySourcePrivate { get; } = new();
        internal async Task FillHistoryAsync(int? maxLimit = null)
        {
            HistoryLoading = true;
            await HistorySourcePrivate.FillHistoryAsync(maxLimit);
            HistoryLoading = false;
        }
    }
}
