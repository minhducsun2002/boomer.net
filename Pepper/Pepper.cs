using System;
using System.Net;
using System.Net.Http;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pepper.Commons;
using Pepper.Commons.Maimai;
using Pepper.Commons.Osu;
using Pepper.Commons.Structures;
using Pepper.Database;
using Pepper.Database.MaimaiDxNetCookieProviders;

DotEnv.Load();
var webhookLog = Environment.GetEnvironmentVariable("PEPPER_DISCORD_WEBHOOK_LOG");

var hostBuilder = new HostBuilder()
    .UseLogging(webhookLog)
    .ConfigureAppConfiguration(app =>
    {
        app.AddEnvironmentVariables("PEPPER_");
    })
    .ConfigureServices((context, services) =>
    {
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