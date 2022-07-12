using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commands.Osu;
using Pepper.Commons.Osu;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Structures.Commands;
using Pepper.Structures.External.Osu;
using Qmmands;

namespace Pepper.Structures
{
    public partial class Bot
    {
        private static readonly Type[] DownleveledAttributes = { typeof(CategoryAttribute), typeof(PrefixCategoryAttribute) };

        // expose this for tests
        public static void DownlevelAttributes(ModuleBuilder moduleBuilder)
        {
            foreach (var command in CommandUtilities.EnumerateAllCommands(moduleBuilder))
            {
                foreach (var attribute in DownleveledAttributes)
                {
                    if (command.Attributes.All(attrib => attrib.GetType() != attribute))
                    {
                        var module = command.Module;
                        while (module != null)
                        {
                            var category = module.Attributes.FirstOrDefault(attrib => attrib.GetType() == attribute);
                            if (category != null)
                            {
                                command.AddAttribute(category);
                                break;
                            }

                            module = module.Parent;
                        }
                    }
                }
            }
        }
        protected override void MutateModule(ModuleBuilder moduleBuilder)
        {
            DownlevelAttributes(moduleBuilder);
            foreach (var command in CommandUtilities.EnumerateAllCommands(moduleBuilder))
            {
                if (command.Parameters.Count == 0)
                {
                    continue;
                }

                // make last non-flag parameters of every command a remainder one
                var lastNotFlag = command.Parameters
                    .LastOrDefault(param => !param.Attributes.OfType<FlagAttribute>().Any());
                if (!(lastNotFlag == null || lastNotFlag.IsRemainder))
                {
                    lastNotFlag.IsRemainder = true;
                }

                // for commands accepting username & game servers, add checks
                if (typeof(OsuCommand).IsAssignableFrom(command.Module.Type))
                {
                    if (command.Parameters.Any(param => param.Type == typeof(GameServer)))
                    {
                        var param = command.Parameters.FirstOrDefault(param => param.Type == typeof(Username));
                        param?.Checks.Add(new EnsureUsernamePresentCheckAttribute());
                        param?.Attributes.Add(new EnsureUsernamePresentCheckAttribute.FailureFormatterAttribute());
                    }
                }
            }



            base.MutateModule(moduleBuilder);
        }

        protected override ValueTask AddTypeParsersAsync(CancellationToken cancellationToken = default)
        {
            Commands.AddTypeParser(new RulesetTypeParser());
            Commands.AddTypeParser(new UsernameTypeParser());
            Commands.AddTypeParser(ActivatorUtilities.CreateInstance<BeatmapOrSetResolvableTypeParser>(Services));
            Commands.AddTypeParser(ActivatorUtilities.CreateInstance<BeatmapResolvableTypeParser>(Services));
            Commands.AddTypeParser(ActivatorUtilities.CreateInstance<GameServerTypeParser>(Services), true);

            return base.AddTypeParsersAsync(cancellationToken);
        }
    }
}