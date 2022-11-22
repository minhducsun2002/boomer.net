using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;

namespace Pepper.Commons.Osu
{
    public class OsuOAuth2Authenticator : AuthenticatorBase
    {
        private static readonly RestClient RestClient = new RestClient(new RestClientOptions("https://osu.ppy.sh/")).UseNewtonsoftJson();

#pragma warning disable CS8618
        private class AuthorizationResponse
        {
            [JsonProperty("expires_in")] public int ExpireInSeconds { get; set; }
            [JsonProperty("access_token")] public string AccessToken { get; set; }
        }
#pragma warning disable CS8618


        private readonly string clientSecret;
        private readonly int clientId;
        private string token { get; set; }
        private DateTimeOffset expiration = DateTimeOffset.Now;

        public OsuOAuth2Authenticator(int clientId, string clientSecret) : base("")
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        {
            var result = token;
            if (expiration < DateTimeOffset.Now)
            {
                var request = new RestRequest("oauth/token", Method.Post)
                {
                    RequestFormat = DataFormat.Json
                }
                    .AddJsonBody(new
                    {
                        grant_type = "client_credentials",
                        scope = "public",
                        client_id = clientId,
                        client_secret = clientSecret
                    });
                var response = await RestClient.PostAsync<AuthorizationResponse>(request);
                expiration = DateTimeOffset.Now + TimeSpan.FromSeconds(response!.ExpireInSeconds - 30);
                result = token = response.AccessToken;
            }

            return new HeaderParameter(KnownHeaders.Authorization, "Bearer " + result);
        }
    }
}