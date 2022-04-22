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

            var credentials = new Credentials();
            configureCredentials.Invoke(credentials);

            if (credentials.OAuth2ClientId == default)
            {
                throw new InvalidOperationException($"Value of {nameof(credentials.OAuth2ClientId)} is invalid.");
            }

            if (credentials.OAuth2ClientSecret == null)
            {
                throw new InvalidOperationException($"{nameof(credentials.OAuth2ClientSecret)} is null.");
            }

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