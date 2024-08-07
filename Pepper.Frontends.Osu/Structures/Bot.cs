using Disqord;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using osu.Game.Rulesets;
using Pepper.Commons.Osu;
using Pepper.Frontends.Database.OsuUsernameProviders;
using Pepper.Frontends.Osu.Commands;
using Pepper.Frontends.Osu.Structures.TypeParsers;
using Qmmands;
using Qmmands.Default;
using Qmmands.Text;

namespace Pepper.Frontends.Osu.Structures
{
    public class Bot : Commons.Structures.Bot
    {
        public Bot(
            IOptions<DiscordBotConfiguration> options,
            ILogger<Bot> logger,
            IServiceProvider services,
            DiscordClient client
        ) : base(options, logger, services, client)
        { }

        protected override void MutateTopLevelModule(IModuleBuilder moduleBuilder)
        {
            // for commands accepting username & game servers, add checks
            foreach (var command in CommandUtilities.EnumerateAllCommands(moduleBuilder).OfType<ITextCommandBuilder>())
            {
                if (typeof(OsuCommand).IsAssignableFrom(command.Module.TypeInfo))
                {
                    if (command.Parameters.Any(param => param.ReflectedType == typeof(GameServer)))
                    {
                        var param = command.Parameters.FirstOrDefault(param => param.ReflectedType == typeof(Username));
                        param?.Checks.Add(new EnsureUsernamePresentCheckAttribute());
                    }

                    if (command.Parameters.Any(param => param.ReflectedType == typeof(Ruleset)))
                    {
                        var param = command.Parameters.FirstOrDefault(param => param.ReflectedType == typeof(Ruleset));
                        param?.Checks.Add(new FillUnknownRulesetAttribute());
                    }
                }
            }

            base.MutateTopLevelModule(moduleBuilder);
        }

        protected override ValueTask AddTypeParsers(DefaultTypeParserProvider typeParserProvider, CancellationToken cancellationToken)
        {
            typeParserProvider.AddParserAsDefault(new RulesetTypeParser());
            typeParserProvider.AddParserAsDefault(new UsernameTypeParser());
            typeParserProvider.AddParserAsDefault(ActivatorUtilities.CreateInstance<BeatmapOrSetResolvableTypeParser>(Services));
            typeParserProvider.AddParserAsDefault(ActivatorUtilities.CreateInstance<BeatmapResolvableTypeParser>(Services));
            typeParserProvider.AddParserAsDefault(ActivatorUtilities.CreateInstance<GameServerTypeParser>(Services));
            return base.AddTypeParsers(typeParserProvider, cancellationToken);
        }
    }
}