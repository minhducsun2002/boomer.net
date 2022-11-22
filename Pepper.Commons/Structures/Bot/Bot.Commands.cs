using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Structures.CommandAttributes.Metadata;
using Qmmands;
using Qmmands.Text;
using Qmmands.Text.Default;

namespace Pepper.Commons.Structures
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
            }

            base.MutateModule(moduleBuilder);
        }

        protected override ValueTask OnInitialize(CancellationToken cancellationToken)
        {
            var argumentParserProvider = (DefaultArgumentParserProvider) Services.GetRequiredService<IArgumentParserProvider>();
            argumentParserProvider.Add(new ArgumentParser());
            argumentParserProvider.SetDefaultParser(typeof(ArgumentParser));
            return base.OnInitialize(cancellationToken);
        }

        protected override ValueTask<bool> ShouldInitializeApplicationCommands(CancellationToken cancellationToken)
        {
            return new ValueTask<bool>(false);
        }

        protected override IEnumerable<Assembly> GetModuleAssemblies()
        {
            var commonAssembly = typeof(Bot).Assembly;
            return base.GetModuleAssemblies().Append(commonAssembly);
        }
    }
}