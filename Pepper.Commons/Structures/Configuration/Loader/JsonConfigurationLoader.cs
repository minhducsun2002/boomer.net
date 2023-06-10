using System.Text.Json;
using Pepper.Commons.Interfaces;

namespace Pepper.Commons.Structures.Configuration.Loader
{
    public class JsonConfigurationLoader<T> : IConfigurationLoader where T : GlobalConfiguration
    {
        private static readonly HttpClient HttpClient = new();
        public async Task<GlobalConfiguration> Load(string path)
        {
            string f;
            try
            {
                var uri = new Uri(path);
                if (uri.Scheme.ToLowerInvariant() is "http" or "https")
                {
                    Console.WriteLine("Trying to download from {0}", uri);
                    f = await HttpClient.GetStringAsync(uri);
                }
                else
                {
                    throw new UriFormatException();
                }
            }
            catch (UriFormatException)
            {
                Console.WriteLine("Reading as file from {0}", path);
                f = await File.ReadAllTextAsync(path);
            }

            var json = JsonSerializer.Deserialize<T>(f);
            if (json == null)
            {
                throw new ArgumentException("Configuration deserialization failed!");
            }
            return json;
        }
    }
}