using System.Diagnostics;
using dotenv.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Pepper.Commons.Services;
using Pepper.Commons.Structures.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Discord;
using Serilog.Templates;

namespace Pepper.Commons
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseLogging(this IHostBuilder hostBuilder, string? discordWebhookEndpoint = null)
        {
            hostBuilder = hostBuilder.UseSerilog((_, configuration) =>
            {
                var loggingTemplate = new ExpressionTemplate(
                    "[{@t:dd-MM-yy HH:mm:ss} {@l:u3}] [Thread {ThreadId,2}]{#if SourceContext is not null} [{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}]{#end} {@m:lj}\n{@x}{#if Contains(@x, 'Exception')}\n{#end}"
                );
                configuration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Infrastructure", LogEventLevel.Warning)
                    .Enrich.WithThreadId()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(loggingTemplate)
                    .WriteTo.Debug(loggingTemplate, restrictedToMinimumLevel: LogEventLevel.Error);

                if (discordWebhookEndpoint != null)
                {
                    var uri = new Uri(discordWebhookEndpoint);
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
            });
            return hostBuilder;
        }

        public static IHostBuilder UseDefaultServices(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(collection =>
            {
                collection.AddHttpClient();
                collection.AddSingleton<HostService>();
            });
        }

        public static IHostBuilder UseEnvironmentVariables(this IHostBuilder hostBuilder)
        {
            const string prefix = "PEPPER_";
            var env = DotEnv.Read()
                .ToDictionary(kv => kv.Key[prefix.Length..], kv => kv.Value);
            hostBuilder = hostBuilder.ConfigureAppConfiguration(app =>
            {
                app.AddInMemoryCollection(env);
                app.AddEnvironmentVariables(prefix);
            });
            return hostBuilder;
        }
    }
}