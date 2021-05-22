using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using dotenv.net;
using Interactivity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using Newtonsoft.Json;
using Pepper.Structures;
using Serilog;
using Serilog.Templates;

namespace Pepper
{
    public partial class Pepper
    {
        private static void PreInitialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    new ExpressionTemplate(
                        "[{@t:dd-MM-yy HH:mm:ss} {@l:u3}] [Thread {ThreadId}]{#if Contains(SourceContext, 'Service')} [{SourceContext}]{#end} {@m:lj}\n{@x}"
                    )
                )
                .CreateLogger();
            Log.Information($"Pepper, built from commit {VersionHash}, initializing...");
            DotEnv.Load();
            const string config = "./config/config.json";
            ConventionRegistry.Register(
                "IgnoreExtraElements",
                new ConventionPack { new IgnoreExtraElementsConvention(true) },
                type => true
            );
            try
            {
                var _ = File.ReadAllText(config, Encoding.UTF8);
                configuration = JsonConvert.DeserializeObject<Configuration>(_)!;
                Log.Information("Loaded configuration from {0}.", config);
            }
            catch (Exception exception)
            {
                Log.Error("Reading configuration from {0} failed", config);
                Log.Error(exception, "[{Timestamp:HH:mm:ss} {Level}] ({ThreadId}) {Message}{NewLine}{Exception} {Properties:j}");
            }
        }

        private static Task InitializeServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(configuration)
                .AddSingleton(new InteractivityService(Client));
            serviceTypes = Assembly.GetEntryAssembly()!.GetTypes()
                .Where(type => typeof(Service).IsAssignableFrom(type) && !type.IsAbstract &&
                               !type.ContainsGenericParameters).ToArray();
            foreach (var service in serviceTypes) services.AddSingleton(service);
            serviceProvider = services.BuildServiceProvider();
            
            var loadedServices = serviceTypes.SelectMany(serviceProvider.GetServices).ToImmutableArray();
            Log.Information($"Loaded {loadedServices.Length} services.");
            foreach (var service in loadedServices) Log.Debug($@"Loaded service {service!.GetType().FullName}.");

            Client.Ready += () =>
            {
                Log.Information($"Logged in as {Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}.");
                return Task.CompletedTask;
            };
            
            return Task.WhenAll(
                serviceTypes.Select(type =>
                {
                    var service = serviceProvider.GetService(type) as Service;
                    Log.Debug($"Initializing service {service!.GetType().FullName}...");
                    return service.Initialize();
                })
            );
        }
    }
}