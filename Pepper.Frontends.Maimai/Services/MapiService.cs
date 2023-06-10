using System.Net.Http.Json;
using Pepper.Frontends.Maimai.Structures.External.Mapi;

namespace Pepper.Frontends.Maimai.Services
{
    public class MapiService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly Uri baseEndpoint;
        public MapiService(IHttpClientFactory httpClientFactory, string baseEndpoint)
        {
            this.httpClientFactory = httpClientFactory;
            this.baseEndpoint = new Uri(baseEndpoint);
        }

        public async Task<FriendResponse> Get(string friendCode)
        {
            var path = new Uri(baseEndpoint, "/friend/" + friendCode);
            var client = httpClientFactory.CreateClient();
            var res = await client.GetFromJsonAsync<FriendResponse>(path).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(res);
            return res;
        }
    }
}