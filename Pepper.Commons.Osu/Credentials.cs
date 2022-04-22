namespace Pepper.Commons.Osu
{
    public class Credentials
    {
        public int OAuth2ClientId { get; set; }
#pragma warning disable CS8618
        public string OAuth2ClientSecret { get; set; }
#pragma warning restore CS8618
        public string? LegacyAPIKey { get; set; }
    }
}