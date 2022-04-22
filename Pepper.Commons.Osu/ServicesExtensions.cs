using System;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Osu.APIClients.Default;

namespace Pepper.Commons.Osu
{
    public static class ServicesExtension
    {
        public static IServiceCollection AddAPIClientStore(
            this IServiceCollection services,
            Action<Credentials>? configureCredentials)
        {
            if (configureCredentials == null)
            {
                throw new ArgumentNullException(nameof(configureCredentials));
            }

            var credentials = new Credentials
            {
                OAuth2ClientSecret = ""
            };
            configureCredentials.Invoke(credentials);

            services.AddSingleton(OsuRestClientBuilder.Build(
                credentials.OAuth2ClientId,
                credentials.OAuth2ClientSecret
            ));

            if (credentials.LegacyAPIKey != null)
            {
                services.AddSingleton(new DefaultOsuAPIClient.LegacyAPIToken(credentials.LegacyAPIKey));
            }
            services.AddSingleton<APIClientStore>();

            return services;
        }
    }
}