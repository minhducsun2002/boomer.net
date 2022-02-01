using Newtonsoft.Json;

namespace Pepper.Commons.Osu
{
#pragma warning disable CS8618
    public class OsuOAuth2Credentials
    {
        [JsonProperty("client_id")] public int ClientId { get; set; }
        [JsonProperty("client_secret")] public string ClientSecret { get; set; }
    }
#pragma warning restore CS8618
}