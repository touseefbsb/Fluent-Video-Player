using System;
using Fluent_Video_Player.Services;
using Fluent_Video_Player.Database;
using Microsoft.EntityFrameworkCore;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Fluent_Video_Player.Helpers;
using Windows.UI.Xaml.Media.Animation;
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Threading.Tasks;

namespace Fluent_Video_Player
{
    public sealed partial class App : Application
    {
        private Lazy<ActivationService> _activationService;
        private ActivationService ActivationService => _activationService.Value;

        public App()
        {
            InitializeComponent();
            EnteredBackground += App_EnteredBackground;
            Current.UnhandledException += Current_UnhandledException;
            using (var db = new DatabaseContext()) { db.Database.Migrate(); }
            ConnectedAnimationService.GetForCurrentView().DefaultDuration = Constants.ConnectedAnimationDuration;
            AppCenter.Start(Constants.AppCenterKey, typeof(Analytics), typeof(Crashes));

            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!args.PrelaunchActivated)
            {
                SystemInformation.TrackAppUse(args);
                await ActivationService.ActivateAsync(args);
            }
        }
        protected override async void OnActivated(IActivatedEventArgs args) => await ActivationService.ActivateAsync(args);
        protected override async void OnFileActivated(FileActivatedEventArgs args) => await ActivationService.ActivateAsync(args);

        private ActivationService CreateActivationService() => new ActivationService(this, typeof(Views.HomePage), new Lazy<UIElement>(CreateShell));

        private UIElement CreateShell() => new Views.ShellPage();

        private async void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            var deferral = e.GetDeferral();
            try
            {
                Task delay = Task.Delay(2000);
                await Task.WhenAny(delay, Helpers.Singleton<SuspendAndResumeService>.Instance.SaveStateAsync());
            }
            catch
            { }
            finally
            {
                deferral.Complete();
            }
            //await Helpers.Singleton<SuspendAndResumeService>.Instance.SaveStateAsync();
        }
        private void Current_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            var exceptionProps = new Dictionary<string, string>
            {
                { "ExceptionMessage", e.Message },
                { "StackTrace", e.Exception.StackTrace }
            };
            Analytics.TrackEvent("UnhandledExceptionOccured", exceptionProps);
            e.Handled = true;
        }
    }
}
