using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using OsuSharp;
using Pepper.Commons.Osu;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Services;
using Pepper.Structures;
using Pepper.Structures.Commands;
using Prometheus;
using Prometheus.DotNetRuntime;
using Qmmands;
using Serilog;
using Serilog.Templates;

DotEnv.Load();
var hostBuilder = new HostBuilder()
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
    .ConfigureAppConfiguration(app =>
    {
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
    .ConfigureServices((context, services) =>
    {
        var enableMetrics = Environment.GetEnvironmentVariable("ENABLE_METRICS");
        var portEnv = Environment.GetEnvironmentVariable("PORT") ?? "2827";
        if (enableMetrics == "true" && int.TryParse(portEnv, out var port))
        {
            DotNetRuntimeStatsBuilder.Default().StartCollecting();
            var server = new MetricServer(port).Start();
            services.AddSingleton(server);
        }

        services.AddScoped<TypeParsedArgumentPersistenceService>();
        services.AddDbContext<IOsuUsernameProvider, MariaDbOsuUsernameProvider>(builder =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING")!;
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });

        services.AddSingleton(
            new OsuClient(new OsuSharpConfiguration
            {
                ApiKey = context.Configuration["OSU_API_KEY"]
            })
        );
        services.AddSingleton<ModParserService>();
        services.AddSingleton(new OsuOAuth2Credentials
        {
            ClientId = int.Parse(context.Configuration["OSU_OAUTH2_CLIENT_ID"]),
            ClientSecret = context.Configuration["OSU_OAUTH2_CLIENT_SECRET"]
        });
        services.AddSingleton<HttpClient>();
        services.AddSingleton<APIClientStore>();
        services.Configure<CommandServiceConfiguration>(config =>
        {
            config.DefaultRunMode = RunMode.Parallel;
            config.DefaultArgumentParser = new ArgumentParser();
            config.IgnoresExtraArguments = true;
            config.StringComparison = StringComparison.InvariantCultureIgnoreCase;
        });
    })
    .ConfigureDiscordBot<Pepper.Pepper>((context, bot) =>
    {
        bot.Token = context.Configuration["DISCORD_TOKEN"];
        bot.Prefixes = context.Configuration.GetAllCommandPrefixes()
            .SelectMany(kv => kv.Value);
    })
    .UseDefaultServiceProvider(option => option.ValidateOnBuild = true);

await hostBuilder.RunConsoleAsync();

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
        ) : base(options, logger, services, client) { }
    }
}
