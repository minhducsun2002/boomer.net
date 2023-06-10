using System.Diagnostics;
using System.Reflection;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pepper.Commons;
using Pepper.Commons.Osu;
using Pepper.Commons.Structures;
using Pepper.Commons.Structures.Configuration.Loader;
using Pepper.Frontends.Database.OsuUsernameProviders;
using Pepper.Frontends.Osu.Structures.Configuration;
using Serilog;
using Bot = Pepper.Frontends.Osu.Structures.Bot;

Debug.Assert(args.Length > 0);
var host = HostManager.Create<GlobalConfiguration>((host, config) =>
{
    host.UseEnvironmentVariables()
        .ConfigureServices((_, services) =>
        {
            services.AddScoped<IOsuUsernameProvider, MariaDbOsuUsernameProvider>(services =>
            {
                var logger = services.GetRequiredService<ILogger>();
                var builder = new DbContextOptionsBuilder<MariaDbOsuUsernameProvider>();
                var connectionString = config.Database!.Main!;
                builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

                return new MariaDbOsuUsernameProvider(builder.Options, logger);
            });

            services.AddSingleton<ModParserService>();
            services.AddAPIClientStore(credentials =>
            {
                credentials.OAuth2ClientId = config.Osu!.V2!.ClientId;
                credentials.OAuth2ClientSecret = config.Osu.V2.ClientSecret!;
                credentials.LegacyAPIKey = config.Osu.V1ApiKey;
            });
        })
        .ConfigureDiscordBot<Bot>((context, bot) =>
        {
            bot.Token = context.Configuration["DISCORD_TOKEN"];
            bot.Prefixes = new[] { "o!" };
            bot.Intents |= GatewayIntents.DirectMessages;
            bot.ServiceAssemblies = new[]
            {
                Assembly.GetEntryAssembly()!, typeof(Service).Assembly
            };
        })
        .UseDefaultServiceProvider(option => option.ValidateOnBuild = true);
}, new JsonConfigurationLoader<GlobalConfiguration>(), args[0]);

await host.Run();