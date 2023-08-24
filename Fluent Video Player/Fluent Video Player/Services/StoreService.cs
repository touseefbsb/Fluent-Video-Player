using Fluent_Video_Player.Contracts.Services;
using Fluent_Video_Player.Extensions;
using Newtonsoft.Json.Linq;
using Windows.Services.Store;
using Windows.Storage;

namespace Fluent_Video_Player.Services
{
    public class StoreService : IStoreService
    {
        public async Task RateAsync()
        {
            var result = await StoreRequestHelper.SendRequestAsync(StoreContext.GetDefault(), 16, string.Empty);
            if (result.ExtendedError is null)
            {
                var jsonObject = JObject.Parse(result.Response);
                if (jsonObject?.SelectToken("status")?.ToString() == "success")
                {
                    await ApplicationData.Current.RoamingSettings.SaveAsync("hasRated", true);
                }
            }
        }
    }
}
