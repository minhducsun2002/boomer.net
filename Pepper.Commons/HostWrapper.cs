using dotenv.net;
using Microsoft.Extensions.Hosting;
using Pepper.Commons.Interfaces;
using Pepper.Commons.Structures.Configuration;

namespace Pepper.Commons
{
    public class HostWrapper<TConfiguration> where TConfiguration : GlobalConfiguration
    {
        private IHost? host;
        private readonly Action<IHostBuilder, TConfiguration> configurationCallback;
        private TConfiguration? currentConfiguration;
        private readonly IConfigurationLoader<TConfiguration> configurationLoader;
        private readonly string configurationPath;

        static HostWrapper()
        {
            DotEnv.Load();
        }

        public HostWrapper(
            Action<IHostBuilder, TConfiguration> configurationCallback,
            IConfigurationLoader<TConfiguration> loader,
            string configurationPath
        )
        {
            this.configurationCallback = configurationCallback;
            configurationLoader = loader;
            this.configurationPath = configurationPath;
        }

        public async Task Run()
        {
            if (host == null)
            {
                await PrepareHost();
            }
            await host.RunAsync();
        }

        private async Task PrepareHost()
        {
            currentConfiguration = await configurationLoader.Load(configurationPath);
            var hostBuilder = new HostBuilder()
                .UseConsoleLifetime()
                .UseLogging(currentConfiguration.Logging?.Discord)
                .UseDefaultServices();
            configurationCallback.Invoke(hostBuilder, currentConfiguration);

            host = hostBuilder.Build();
        }
    }
}