namespace Pepper.Commons.Osu
{
    public class Credentials
    {
        public int OAuth2ClientId { get; set; }
        public string OAuth2ClientSecret { get; set; }
        public string? LegacyAPIKey { get; set; }
    }
}