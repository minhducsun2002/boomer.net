using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Pepper.Commons.Osu
{
    public class APIClientStore
    {
        private Dictionary<GameServer, APIClient> apiClients = new();
        public APIClientStore(IServiceProvider serviceProvider)
        {
            var assembly = typeof(APIClient).Assembly;
            foreach (var type in assembly.ExportedTypes)
            {
                if (typeof(APIClient).IsAssignableFrom(type))
                {
                    var attribs = type.GetCustomAttribute<GameServerAttribute>();
                    if (attribs != null)
                    {
                        var client = (APIClient) ActivatorUtilities.CreateInstance(serviceProvider, type);
                        apiClients[attribs.Server] = client;
                    }
                }
            }
        }

        public APIClient GetClient(GameServer server) => apiClients[server];
    }
}