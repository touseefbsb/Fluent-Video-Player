using Fluent_Video_Player.Activation;
using Fluent_Video_Player.Helpers;
using Fluent_Video_Player.Views;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace Fluent_Video_Player.Services
{
    internal class FileAssociationService : ActivationHandler<File​Activated​Event​Args>
    {
        protected override async Task HandleInternalAsync(File​Activated​Event​Args args)
        {

            await MediaHelper.MyMediaPlayer.SetNewSourceWithFiles(args.Files, args.NeighboringFilesQuery);
            var result = NavigationService.Navigate(typeof(Views.PlayerPage));
            if (!result)
                PlayerPage.VM.Initialize();
            await Task.CompletedTask;
        }
    }
}
