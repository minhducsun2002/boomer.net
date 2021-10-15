using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using dotenv.net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using Pepper.Services.FGO;
using Pepper.Structures;
using Pepper.Structures.Commands;
using Pepper.Structures.External.FGO.TypeParsers;
using Pepper.Structures.External.Osu;
using Qmmands;
using Serilog;
using Serilog.Templates;

await new HostBuilder()
    .UseSerilog((_, configuration) =>
        configuration
            .MinimumLevel.Information()
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                new ExpressionTemplate(
                    "[{@t:dd-MM-yy HH:mm:ss} {@l:u3}] [Thread {ThreadId,2}]{#if SourceContext is not null} [{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}]{#end} {@m:lj}\n{@x}{#if Contains(@x, 'Exception')}\n{#end}"
                )
            )
    )
    .ConfigureServices(services =>
    {
        services.Configure<CommandServiceConfiguration>(config =>
        {
            config.DefaultRunMode = RunMode.Parallel;
            config.DefaultArgumentParser = new ArgumentParser();
            config.IgnoresExtraArguments = true;
            config.StringComparison = StringComparison.InvariantCultureIgnoreCase;
        });
    })
    .ConfigureAppConfiguration(app =>
    {
        DotEnv.Load();
        app.AddEnvironmentVariables("PEPPER_");
        ConventionRegistry.Register(
            "IgnoreExtraElements",
            new ConventionPack { new IgnoreExtraElementsConvention(true) },
            _ => true
        );
        
        var configUrl = Environment.GetEnvironmentVariable("PEPPER_CONFIG_URL");
        if (!string.IsNullOrWhiteSpace(configUrl))
        {
            var httpClient = new HttpClient();
            Log.Information($"Downloading JSON configuration from {configUrl}...");
            var config = httpClient.GetStreamAsync(configUrl).Result;
            app.AddJsonStream(config);
        }
        else
        {
            app.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config/config.json"));
        }
    })
    .ConfigureDiscordBot<Pepper.Pepper>((context, bot) =>
    {
        bot.Token = context.Configuration["DISCORD_TOKEN"];
        bot.Prefixes = context.Configuration.GetAllCommandPrefixes()
            .SelectMany(kv => kv.Value);
    })
    .UseDefaultServiceProvider(option => option.ValidateOnBuild = true)
    .RunConsoleAsync();

namespace Pepper
{
    public partial class Pepper : DiscordBot
    {
        public static readonly string VersionHash = typeof(Pepper)
            .Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "GitHash")?.Value ?? "unknown";
        
        public Pepper(
            IOptions<DiscordBotConfiguration> options,
            ILogger<Pepper> logger,
            IServiceProvider services,
            DiscordClient client
        ) : base(options, logger, services, client) {}
    }
}
