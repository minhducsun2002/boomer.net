using System;
using System.Linq;
using System.Reflection;
using Disqord;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pepper.Structures.CommandAttributes.Metadata;
using Qmmands.Text;
using Qmmands.Text.Default;

namespace Pepper.Structures
{
    public partial class Bot : DiscordBot
    {
        public static readonly string VersionHash = typeof(Bot)
            .Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "GitHash")?.Value ?? "unknown";

        public Bot(
            IOptions<DiscordBotConfiguration> options,
            ILogger<Bot> logger,
            IServiceProvider services,
            DiscordClient client
        ) : base(options, logger, services, client)
        { }
    }
}
