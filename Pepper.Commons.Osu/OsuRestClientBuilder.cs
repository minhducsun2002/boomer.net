using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Pepper.Commons.Osu
{
    public class OsuRestClientBuilder
    {
        public static OsuRestClient Build(int clientId, string clientSecret)
        {
            var client = new OsuRestClient(new RestClientOptions("https://osu.ppy.sh/api/v2")
            {
                ThrowOnAnyError = true
            })
            {
                Authenticator = new OsuOAuth2Authenticator(
                    clientId,
                    clientSecret
                )
            };
            return (OsuRestClient) client.UseNewtonsoftJson(new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            });
        }
    }
}