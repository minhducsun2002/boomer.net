namespace Pepper.Commons.Osu.APIClients.Default
{
    public partial class DefaultOsuAPIClient
    {
        public class LegacyAPIToken
        {
            internal readonly string Token;
            public LegacyAPIToken(string token)
            {
                Token = token;
            }
        }
    }
}