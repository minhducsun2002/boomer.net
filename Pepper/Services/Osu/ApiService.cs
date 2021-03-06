using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OsuSharp;
using Pepper.Structures;

namespace Pepper.Services.Osu
{
    public partial class ApiService : Service
    {
        public ApiService(IConfiguration configuration)
        {
            legacyApiClient = new OsuClient(new OsuSharpConfiguration
            {
                ApiKey = configuration["OSU_API_KEY"]
            });
        }
        private static readonly JsonSerializerSettings DefaultSerializerSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        private readonly HttpClient httpClient = new()
        {
            DefaultRequestHeaders =
            { UserAgent = { new ProductInfoHeaderValue("Pepper", Pepper.VersionHash) } }
        };

        private readonly OsuClient legacyApiClient;
    }
}