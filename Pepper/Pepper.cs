using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Disqord;
using Disqord.Bot.Hosting;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pepper.Commons.Maimai;
using Pepper.Commons.Osu;
using Pepper.Database;
using Pepper.Database.MaimaiDxNetCookieProviders;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Logging.Serilog.Sinks.Discord;
using Pepper.Services;
using Pepper.Structures;
using Qmmands;
using Qmmands.Text;
using Qmmands.Text.Default;
using Serilog;
using Serilog.Events;
using Serilog.Templates;

DotEnv.Load();
var webhookLog = Environment.GetEnvironmentVariable("PEPPER_DISCORD_WEBHOOK_LOG");

var hostBuilder = new HostBuilder()
    .UseSerilog((_, configuration) =>
    {
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Infrastructure", LogEventLevel.Warning)
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
        services.AddDbContextPool<IOsuUsernameProvider, MariaDbOsuUsernameProvider>(builder =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING")!;
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }, 16);
        services.AddDbContextPool<IMaimaiDxNetCookieProvider, MariaDbMaimaiDxNetCookieProvider>(builder =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING")!;
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }, 16);
        services.AddDbContextPool<MaimaiDbContext>(builder =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING_MAIMAI")!;
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }, 16);
        services.AddDbContextPool<RestrictedCommandWhitelistProvider>(builder =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING")!;
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }, 16);

        services.AddSingleton<ModParserService>();
        services.AddSingleton<HttpClient>();
        services.AddAPIClientStore(credentials =>
        {
            credentials.OAuth2ClientId = int.Parse(context.Configuration["OSU_OAUTH2_CLIENT_ID"]);
            credentials.OAuth2ClientSecret = context.Configuration["OSU_OAUTH2_CLIENT_SECRET"];
            credentials.LegacyAPIKey = context.Configuration["OSU_API_KEY"];
        });
    })
    .ConfigureDiscordBot<Bot>((context, bot) =>
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