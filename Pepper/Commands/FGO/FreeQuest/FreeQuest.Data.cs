using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pepper.Services.FGO;

namespace Pepper.Commands.FGO
{
    public partial class FreeQuest
    {
        private const string Endpoint = "https://rayshift.io/api/v1/avalon-data-export/quests/";
        private static readonly string APIKey = Environment.GetEnvironmentVariable("RAYSHIFT_IO_API_KEY") ?? "";
        private static readonly HttpClient HttpClient = new();

        private static async Task<Dictionary<int, Quest>?> Query(Region region, int questId, int questPhase)
        {
            var url = Endpoint + $"get?questId={questId}&questPhase={questPhase}&region={(int)region}&apiKey={APIKey}";
            var raw = await HttpClient.GetStringAsync(url);
            var obj = JObject.Parse(raw);
            return obj["status"]!.ToObject<int>() == 404 
                ? null
                : obj["response"]!["questDetails"]!.ToObject<Dictionary<int, Quest>>()!;
        }
    }
}