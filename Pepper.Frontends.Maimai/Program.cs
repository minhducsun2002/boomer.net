using System.Reflection;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pepper.Commons;
using Pepper.Commons.Maimai;
using Pepper.Commons.Structures;
using Pepper.Commons.Structures.Configuration.Loader;
using Pepper.Frontends.Maimai.Database;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Structures.Configuration;
using Serilog;

var path = args.ElementAtOrDefault(0) ?? Environment.GetEnvironmentVariable("CONFIG_PATH");
ArgumentNullException.ThrowIfNull(path);

var host = HostManager.Create<GlobalConfiguration>((host, config) =>
{
    host.UseEnvironmentVariables()
        .ConfigureServices((_, services) =>
        {
            services.AddScoped<IMaimaiDxNetCookieProvider, MariaDbMaimaiDxNetCookieProvider>(services =>
            {
                var connectionString = config.Database!.Main!;
                var builder = new DbContextOptionsBuilder<MariaDbMaimaiDxNetCookieProvider>();
                builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

                return new MariaDbMaimaiDxNetCookieProvider(builder.Options, services.GetRequiredService<ILogger>());
            });
            services.AddDbContextPool<MaimaiDataDbContext>(builder =>
            {
                var connectionString = config.Database!.Maimai!;
                builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }, 16);
            services.AddMaimaiDxNetClient();
            services.AddSingleton(s =>
            {
                var factory = s.GetRequiredService<IHttpClientFactory>();
                return new MapiService(factory, config.Maimai!.FriendApiServer!);
            });
            services.AddSingleton<ICookieConsistencyLocker, CookieLockingService>();
        })
        .ConfigureDiscordBot<Bot>((context, bot) =>
        {
            bot.Token = context.Configuration["DISCORD_TOKEN"];
            bot.Prefixes = new[] { "m!" };
            bot.Intents |= GatewayIntents.DirectMessages;
            bot.ServiceAssemblies = new[]
            {
                Assembly.GetEntryAssembly()!, typeof(Service).Assembly
            };
        })
        .UseDefaultServiceProvider(options => options.ValidateOnBuild = true);
}, new JsonConfigurationLoader<GlobalConfiguration>(), path);

await host.Run();


