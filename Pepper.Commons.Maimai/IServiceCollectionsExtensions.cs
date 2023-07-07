using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace Pepper.Commons.Maimai
{
    public static class ServiceCollectionsExtensions
    {
        public static IServiceCollection AddMaimaiDxNetClient(this IServiceCollection collection)
        {
            collection
                .AddHttpClient(MaimaiDxNetClientFactory.LoginFactoryName)
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    AllowAutoRedirect = false
                });
            collection.AddHttpClient(MaimaiDxNetClientFactory.ClientFactoryName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    UseCookies = true
                });
            collection.AddSingleton<MaimaiDxNetClientFactory>();
            return collection;
        }
    }
}