using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commands.Osu;
using Pepper.Commons.Osu;
using Pepper.Commons.Structures;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Structures.CommandAttributes.Metadata;
using Pepper.Structures.External.Osu;
using Qmmands;
using Qmmands.Default;
using Qmmands.Text;
using Qmmands.Text.Default;

namespace Pepper.Structures
{
    public partial class Bot
    {
        private static readonly Type[] DownleveledAttributeTypes =
        {
            typeof(CategoryAttribute),
            typeof(HiddenAttribute)
        };

        // expose this for tests
        public static void DownlevelAttributes(IModuleBuilder moduleBuilder)
        {
            foreach (var command in CommandUtilities.EnumerateAllCommands(moduleBuilder).OfType<ITextCommandBuilder>())
            {
                foreach (var attributeType in DownleveledAttributeTypes)
                {
                    if (command.CustomAttributes.All(attrib => attrib.GetType() != attributeType))
                    {
                        var module = command.Module;
                        while (module != null)
                        {
                            var category = module.CustomAttributes.FirstOrDefault(attrib => attrib.GetType() == attributeType);
                            if (category != null)
                            {
                                command.CustomAttributes.Add(category);
                                break;
                            }

                            module = module.Parent;
                        }
                    }
                }
            }
        }

        protected override void MutateTopLevelModule(IModuleBuilder moduleBuilder)
        {
            DownlevelAttributes(moduleBuilder);
            foreach (var command in CommandUtilities.EnumerateAllCommands(moduleBuilder).OfType<ITextCommandBuilder>())
            {
                if (command.Parameters.Count == 0)
                {
                    continue;
                }

                // make last non-flag parameters of every command a remainder one
                var lastNotFlag = command.Parameters
                    .LastOrDefault(param => !param.CustomAttributes.OfType<FlagAttribute>().Any());
                if (lastNotFlag is IPositionalParameterBuilder positionalBuilder)
                {
                    positionalBuilder.IsRemainder = true;
                }

                // for commands accepting username & game servers, add checks
                if (typeof(OsuCommand).IsAssignableFrom(command.Module.TypeInfo))
                {
                    if (command.Parameters.Any(param => param.ReflectedType == typeof(GameServer)))
                    {
                        var param = command.Parameters.FirstOrDefault(param => param.ReflectedType == typeof(Username));
                        param?.Checks.Add(new EnsureUsernamePresentCheckAttribute());
                    }
                }
            }

            base.MutateModule(moduleBuilder);
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

        public override ValueTask InitializeAsync(CancellationToken cancellationToken)
        {
            var argumentParserProvider = (DefaultArgumentParserProvider) Services.GetRequiredService<IArgumentParserProvider>();
            argumentParserProvider.Add(new ArgumentParser());
            argumentParserProvider.SetDefaultParser(typeof(ArgumentParser));
            return base.InitializeAsync(cancellationToken);
        }

        public override ValueTask InitializeApplicationCommandsAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }
    }
}