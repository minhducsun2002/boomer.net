using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using OsuSharp;
using Pepper.Structures;

namespace Pepper.Services.Osu
{
    public partial class ApiService : Service
    {
        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        private readonly HttpClient httpClient = new HttpClient
        {
            DefaultRequestHeaders =
            { UserAgent = { new ProductInfoHeaderValue("Pepper", Pepper.VersionHash) } }
        };
        private readonly OsuClient legacyApiClient = new OsuClient(new OsuSharpConfiguration
        {
            ApiKey = Environment.GetEnvironmentVariable("OSU_API_KEY")
        });
    }
}