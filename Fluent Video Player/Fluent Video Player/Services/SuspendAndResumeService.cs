using System;
using System.Threading.Tasks;

using Fluent_Video_Player.Activation;
using Fluent_Video_Player.Database;
using Fluent_Video_Player.Helpers;

using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Fluent_Video_Player.Services
{
    // More details regarding the application lifecycle and how to handle suspend and resume at https://docs.microsoft.com/windows/uwp/launch-resume/app-lifecycle
    internal class SuspendAndResumeService : ActivationHandler<LaunchActivatedEventArgs>
    {
        private const string StateFilename = "SuspendAndResumeState";
        
        public event EventHandler<OnBackgroundEnteringEventArgs> OnBackgroundEntering;

        public async Task SaveStateAsync()
        {
            var suspensionState = new SuspensionState()
            {
                SuspensionDate = DateTime.Now
            };

            var target = OnBackgroundEntering?.Target.GetType();
            var onBackgroundEnteringArgs = new OnBackgroundEnteringEventArgs(suspensionState, target);

            OnBackgroundEntering?.Invoke(this, onBackgroundEnteringArgs);

            await ApplicationData.Current.LocalFolder.SaveAsync(StateFilename, onBackgroundEnteringArgs);
            try
            {
                if (!(MediaHelper.MyMediaPlayer?.CurrentPlaybackList is null))
                {
                    if (!(MediaHelper.MyMediaPlayer?.CurrentPlaybackList?.CurrentItem is null))
                    {
                        if (!(MediaHelper.MyMediaPlayer?.MP?.PlaybackSession?.Position is null))
                        {
                            var currentPosition = (TimeSpan)MediaHelper.MyMediaPlayer?.MP.PlaybackSession.Position;
                            string folderRelativeId = MediaHelper.MyMediaPlayer?.CurrentPlaybackList?.CurrentItem.GetDisplayProperties().VideoProperties.Subtitle;
                            await DbHelper.SaveOrUpdatePosition(folderRelativeId, currentPosition);
                        }
                    }
                }
            }
            catch { }

        }

        protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args) => await RestoreStateAsync();

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args) => args.PreviousExecutionState == ApplicationExecutionState.Terminated;

        private async Task RestoreStateAsync()
        {
            var saveState = await ApplicationData.Current.LocalFolder.ReadAsync<OnBackgroundEnteringEventArgs>(StateFilename);
            if (saveState?.Target != null && typeof(Page).IsAssignableFrom(saveState.Target))
            {
                NavigationService.Navigate(saveState.Target, saveState.SuspensionState);
            }
        }
    }
}
