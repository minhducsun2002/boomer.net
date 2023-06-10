using dotenv.net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pepper.Commons.Interfaces;
using Pepper.Commons.Services;
using Pepper.Commons.Structures.Configuration;

namespace Pepper.Commons
{
    public partial class HostManager
    {
        private IHost? host;
        private readonly Action<IHostBuilder, GlobalConfiguration> configurationCallback;
        private GlobalConfiguration? currentConfiguration;
        private readonly IConfigurationLoader configurationLoader;
        private readonly string configurationPath;
        private readonly Type configurationType;

        static HostManager()
        {
            DotEnv.Load();
        }

        private HostManager(
            Action<IHostBuilder, GlobalConfiguration> configurationCallback,
            IConfigurationLoader loader,
            Type configurationType,
            string configurationPath
        )
        {
            this.configurationCallback = configurationCallback;
            this.configurationType = configurationType;
            configurationLoader = loader;
            this.configurationPath = configurationPath;
        }



        public static HostManager Create<TConfiguration>(
            Action<IHostBuilder, TConfiguration> configurationCallback, IConfigurationLoader loader, string configurationPath
        )
            where TConfiguration : GlobalConfiguration
        {
            void WrappedCallback(IHostBuilder builder, GlobalConfiguration config)
            {
                configurationCallback.Invoke(builder, (TConfiguration) config);
            }

            return new HostManager(WrappedCallback, loader, typeof(TConfiguration), configurationPath);
        }

        private async Task PrepareHost()
        {
            currentConfiguration = await configurationLoader.Load(configurationPath);
            if (!configurationType.IsInstanceOfType(currentConfiguration))
            {
                throw new ArgumentException(
                    $"Expected configuration of type {configurationType} or derivatives - got {currentConfiguration.GetType()}");
            }

            var hostBuilder = new HostBuilder()
                .UseConsoleLifetime()
                .UseLogging(currentConfiguration.Logging?.Discord)
                .UseDefaultServices();
            configurationCallback.Invoke(hostBuilder, currentConfiguration);

            host = hostBuilder.Build();

            using var scope = host.Services.CreateScope();
            var hostWrapper = scope.ServiceProvider.GetService<HostService>();
            if (hostWrapper != null)
            {
                hostWrapper.HostWrapper = this;
            }
        }
    }
}