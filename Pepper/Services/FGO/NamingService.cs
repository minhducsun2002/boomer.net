using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pepper.Structures;
using Serilog;

namespace Pepper.Services.FGO
{
    public abstract class NamingService : Service
    {
        protected static readonly HttpClient HttpClient = new();
        private static readonly ILogger Log = Serilog.Log.Logger.ForContext<NamingService>();
        protected static async Task<List<string>[]> GetCsv(string url, char separator = ',')
        {
            Log.Debug($"Downloading from {url}");
            var response = await HttpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Response status code is {response.StatusCode}. Something is wrong - I'm not importing.");
            }
            var bytes = await response.Content.ReadAsByteArrayAsync();
            Log.Information($"Downloaded {bytes.Length} bytes.");
            return Encoding.UTF8.GetString(bytes).Split('\n')
                .Select(line => line.Split(separator).ToList())
                .ToArray();
        }
    }
}