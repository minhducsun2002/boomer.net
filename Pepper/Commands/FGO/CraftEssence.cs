using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Humanizer;
using Pepper.Services.FGO;
using Pepper.Structures.Commands;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.Renderer;
using Qmmands;

namespace Pepper.Commands.FGO
{
    internal class CEView : ViewBase
    {
        private bool isMLBPage;

        public CEView(
            IList<LocalEmbed> embeds, Snowflake? messageId = null
        ) : base(new LocalMessage { Embeds = new List<LocalEmbed> { embeds[0] } })
        {
            if (messageId != null) TemplateMessage = TemplateMessage.WithReply(messageId.Value);
            if (embeds.Count > 1)
            {
                var button = new ButtonViewComponent(e =>
                {
                    isMLBPage = !isMLBPage;
                    e.Button.Label = isMLBPage ? "Switch to base effect" : "Switch to MLB effects";
                    TemplateMessage.Embeds = new List<LocalEmbed> {embeds[isMLBPage ? 1 : 0]};
                    ReportChanges();
                    return default;
                })
                {
                    Label = "Switch to MLB effect",
                    Style = LocalButtonComponentStyle.Primary,
                    Row = 1, Position = 0
                };
                
                AddComponent(button);
            }
        }
    }
    
    public class CraftEssence : FGOCommand
    {
        public CraftEssence(
            MasterDataService masterDataService,
            ServantNamingService servantNamingService, TraitService traitService, ItemNamingService itemNamingService
        ) : base(masterDataService, servantNamingService, traitService, itemNamingService) {}

        [Command("ce")]
        [PrefixCategory("fgo")]
        public DiscordCommandResult Exec(int id = 1)
        {
            MasterDataMongoDBConnection jp = MasterDataService.Connections[Region.JP], na = MasterDataService.Connections[Region.NA];
            var ce = jp.GetCraftEssenceByCollectionNo(id)!;
            var localizedName = na.GetCraftEssenceByCollectionNo(id)?.MstSvt.Name;
            
            var title = $"{ce.MstSvt.CollectionNo}. {localizedName ?? ce.MstSvt.Name} (`{ce.MstSvt.ID}`)";
            var author = $"Cost : {ce.MstSvt.Cost}";

            var _ = new[] {ce.BaseSkills, ce.MLBSkills}
                .Select(skills => skills.SelectMany(skill =>
                        new SkillRenderer(skill.MstSkill, jp, skill).ResolveEffects(TraitService).Item1
                            .Select(kv => kv.Serialize()))
                    .ToList()).ToArray();
            string baseEffects = string.Join("\n", _[0]), mlbEffects = string.Join("\n", _[1]);
            if (baseEffects.Length > 1020 || mlbEffects.Length > 1020)
            {
                var embeds = new List<LocalEmbed>
                {
                    new()
                    {
                        Author = new LocalEmbedAuthor().WithName(author),
                        Title = title,
                        Description = baseEffects,
                        Footer = new LocalEmbedFooter { Text = $"Base {(ce.BaseSkills.Length > 1 ? "effect".Pluralize() : "effects")}" }
                    }
                };
                if (ce.MLBSkills.Length != 0)
                    embeds.Add(
                        new LocalEmbed
                        {
                            Author = new LocalEmbedAuthor().WithName(author),
                            Title = title,
                            Description = mlbEffects,
                            Footer = new LocalEmbedFooter { Text = $"MLB {(ce.MLBSkills.Length > 1 ? "effect".Pluralize() : "effects")}" }
                        });
                
                return View(new CEView(embeds, Context.Message.Id));
            }

            return Reply(new LocalEmbed
            {
                Author = new LocalEmbedAuthor().WithName(author),
                Title = title,
                Fields = new List<LocalEmbedField?>
                {
                    new()
                    {
                        Name = $"Base {(ce.BaseSkills.Length > 1 ? "effect".Pluralize() : "effects")}",
                        Value = baseEffects
                    },
                    (ce.MLBSkills.Length != 0
                        ? new LocalEmbedField
                        {
                            Name = $"MLB {(ce.BaseSkills.Length > 1 ? "effect".Pluralize() : "effects")}",
                            Value = mlbEffects
                        } 
                        : null)
                }.Where(embed => embed != null).ToList()
            });
        }
    }
}
