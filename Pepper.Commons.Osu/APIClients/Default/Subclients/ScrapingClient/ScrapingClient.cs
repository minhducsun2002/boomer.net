using System.Net.Http;

namespace Pepper.Commons.Osu.APIClients.Default.Subclients
{
    internal partial class ScrapingClient
    {
        private readonly HttpClient httpClient;
        public ScrapingClient(HttpClient httpClient) => this.httpClient = httpClient;
    }
}