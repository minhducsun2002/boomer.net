using System.Reflection;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pepper.Commons;
using Pepper.Commons.Osu;
using Pepper.Commons.Structures;
using Pepper.Frontends.Database.OsuUsernameProviders;
using Bot = Pepper.Frontends.Osu.Structures.Bot;

DotEnv.Load();
var webhookLog = Environment.GetEnvironmentVariable("PEPPER_DISCORD_WEBHOOK_LOG");

var hostBuilder = new HostBuilder()
    .UseLogging(webhookLog)
    .UseEnvironmentVariables()
    .ConfigureServices((context, services) =>
    {
        services.AddDbContextPool<IOsuUsernameProvider, MariaDbOsuUsernameProvider>(builder =>
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
        bot.Token = context.Configuration["DISCORD_TOKEN"];
        bot.Prefixes = new[] { "o!" };
        bot.Intents |= GatewayIntent.DirectMessages;
        bot.ServiceAssemblies = new[]
        {
            Assembly.GetEntryAssembly()!, typeof(Service).Assembly
        };
    })
    .UseDefaultServiceProvider(option => option.ValidateOnBuild = true);

await hostBuilder.RunConsoleAsync();