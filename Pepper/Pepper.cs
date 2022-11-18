using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
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
        services.AddDbContextPool<MaimaiDataDbContext>(builder =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING_MAIMAI")!;
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
        bot.Prefixes = new [] { "b!", "o!" };
        bot.Intents |= GatewayIntent.DirectMessages;
    })
    .UseDefaultServiceProvider(option => option.ValidateOnBuild = true);

await hostBuilder.RunConsoleAsync();