using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fluent_Video_Player.Helpers;
using Windows.UI.Xaml;

namespace Fluent_Video_Player.DataTemplates.ViewModels
{
    public partial class PlaylistTemplateViewModel : ObservableObject
    {
        [ObservableProperty] private bool gridViewwVisibility;
        [ObservableProperty] private bool listViewwVisibility;

        [RelayCommand] private void StateChanged(VisualStateChangedEventArgs args) => GoToState(args.NewState.Name);
        private void TurnToGridView() { GridViewwVisibility = true; ListViewwVisibility = false; }
        private void TurnToListView() { GridViewwVisibility = false; ListViewwVisibility = true; }
        public void Initialize() => InitializeState(Window.Current.Bounds.Width);
        private void InitializeState(double windowWith)
        {
            if (windowWith < Constants.WideStateMinWindowWidth)
            {
                GoToState(Constants.NarrowStateName);
            }
            else if (windowWith < Constants.PanoramicStateMinWindowWidth)
            {
                GoToState(Constants.WideStateName);
            }
            else
            {
                GoToState(Constants.PanoramicStateName);
            }
        }
        private void GoToState(string stateName)
        {
            switch (stateName)
            {
                case Constants.NarrowStateName: TurnToGridView(); break;
                default: TurnToListView(); break;
            }
        }
    }
}
