using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using Pepper.Database;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Logging.Serilog.Sinks.Discord;
using Pepper.Services;
using Pepper.Structures;
using Pepper.Structures.Commands;
using Qmmands;
using Serilog;
using Serilog.Templates;

DotEnv.Load();
var webhookLog = Environment.GetEnvironmentVariable("PEPPER_DISCORD_WEBHOOK_LOG");

var hostBuilder = new HostBuilder()
    .UseSerilog((_, configuration) =>
    {
        configuration
            .MinimumLevel.Information()
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                new ExpressionTemplate(
                    "[{@t:dd-MM-yy HH:mm:ss} {@l:u3}] [Thread {ThreadId,2}]{#if SourceContext is not null} [{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}]{#end} {@m:lj}\n{@x}{#if Contains(@x, 'Exception')}\n{#end}"
                )
            );

        if (webhookLog != null)
        {
            var uri = new Uri(webhookLog);
            Debug.Assert(uri.Host.Contains("discord"));
            var splitted = uri.AbsolutePath.Split('/')
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();

            configuration.WriteTo.DiscordWebhook(
                ulong.Parse(splitted[^2]), splitted[^1],
                formatter: new ExpressionTemplate(
                    "[{@t:dd-MM-yy HH:mm:ss} {@l:u3}] {@m:lj}\n{@x}{#if Contains(@x, 'Exception')}\n{#end}"
                ));
        }
    })
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
        services.AddScoped<TypeParsedArgumentPersistenceService>();
        services.AddDbContext<IOsuUsernameProvider, MariaDbOsuUsernameProvider>(builder =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING")!;
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });
        services.AddDbContextPool<RestrictedCommandWhitelistProvider>(builder =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING")!;
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });

        services.AddSingleton<ModParserService>();
        services.AddSingleton(OsuRestClientBuilder.Build(
            int.Parse(context.Configuration["OSU_OAUTH2_CLIENT_ID"]),
            context.Configuration["OSU_OAUTH2_CLIENT_SECRET"]
        ));

        services.AddSingleton(new OsuClient(new OsuSharpConfiguration
        {
            ApiKey = "1"
        }));
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
        if (context.Configuration["DISCORD_PROXY"] != default)
        {
            bot.GatewayProxy = bot.RestProxy = new WebProxy(context.Configuration["DISCORD_PROXY"]);
        }
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
