using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pepper.Commons.Osu
{
    internal class OsuOAuth2Client
    {
        private class AuthorizationPayload : OsuOAuth2Credentials
        {
            [JsonProperty("grant_type")]
            public const string GrantType = "client_credentials";

            [JsonProperty("scope")] public const string Scope = "public";
        }

        private class AuthorizationResponse
        {
            [JsonPropertyName("expires_in")] public int ExpireInSeconds { get; set; }
            [JsonPropertyName("access_token")] public string AccessToken { get; set; }
        }

        private readonly AuthorizationPayload payload;
        private string Token { get; set; }
        private DateTimeOffset expiration = DateTimeOffset.Now;
        private readonly HttpClient httpClient;

        public OsuOAuth2Client(HttpClient httpClient, OsuOAuth2Credentials oAuth2Credentials)
        {
            this.httpClient = httpClient;
            payload = new AuthorizationPayload
            {
                ClientId = oAuth2Credentials.ClientId,
                ClientSecret = oAuth2Credentials.ClientSecret
            };
        }

        private async Task<string> GetToken()
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload));
            content.Headers.ContentType = new MediaTypeHeaderValue(System.Net.Mime.MediaTypeNames.Application.Json);

            if (expiration < DateTimeOffset.Now)
            {
                var now = DateTimeOffset.Now;
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://osu.ppy.sh/oauth/token"),
                    Content = content,
                    Method = HttpMethod.Post,
                };
                var result = await httpClient.SendAsync(request);
                if (!result.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Failed to obtain osu!api access token", null, result.StatusCode);
                }

                var response = await result.Content.ReadFromJsonAsync<AuthorizationResponse>();
                expiration = now + TimeSpan.FromSeconds(response!.ExpireInSeconds - 30);
                return Token = response.AccessToken;
            }

            return Token;
        }

        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uri)
            };
            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Bearer " + await GetToken());
            return await httpClient.SendAsync(request);
        }
    }
}