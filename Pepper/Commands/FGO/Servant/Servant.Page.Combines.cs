using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Pepper.Structures.External.FGO.Entities;

namespace Pepper.Commands.FGO
{
    public partial class Servant
    {
        private (LocalSelectionComponentOption, Page)? AscensionsPage(BaseServant servant, ServantLimits limits, IReadOnlyDictionary<int, string> itemNames)
        {
            if (limits.AscensionCombine.Length == 0)
            {
                return null;
            }

            var page = servant.BaseEmbed()
                .WithDescription(limits.AscensionCombine.Length == 0 ? "No materials needed." : "")
                .WithFields(
                    limits.AscensionCombine.Select((limit, index) => new LocalEmbedField
                    {
                        Name = $"Stage {index + 1} - {limit.QP.ToString("n0", CultureInfo.InvariantCulture)} QP",
                        Value = string.Join(
                            '\n',
                            limit.ItemIds.Zip(limit.ItemNums)
                                .Select(tuple => $"- **{tuple.Second}**x **{itemNames[tuple.First]}**")
                        )
                    }));

            return (new LocalSelectionComponentOption { Label = "Ascension materials" }, new Page().WithEmbeds(page));
        }

        private (LocalSelectionComponentOption, Page)? SkillLimitsPage(BaseServant servant, ServantLimits limits, IReadOnlyDictionary<int, string> itemNames)
        {
            if (limits.SkillCombine.Length == 0)
            {
                return null;
            }

            var page = servant.BaseEmbed()
                .WithDescription(limits.SkillCombine.Length == 0 ? "No materials needed." : "")
                .WithFields(
                    limits.SkillCombine.Select((limit, index) => new LocalEmbedField
                    {
                        Name = $"Stage {index + 2} - {limit.QP.ToString("n0", CultureInfo.InvariantCulture)} QP",
                        Value = string.Join(
                            '\n',
                            limit.ItemIds.Zip(limit.ItemNums)
                                .Select(tuple => $"- **{tuple.Second}**x **{itemNames[tuple.First]}**")
                        )
                    }));

            return (new LocalSelectionComponentOption { Label = "Skill materials" }, new Page().WithEmbeds(page));
        }
    }
}