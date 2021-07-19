using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
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
        private IList<LocalEmbed> embeds;

        public CEView(IList<LocalEmbed> embeds) : base(new LocalMessage { Embeds = new List<LocalEmbed> { embeds[0] } })
        {
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
        [Command("ce")]
        [PrefixCategory("fgo")]
        public DiscordMenuCommandResult Exec(int id = 1)
        {
            MasterDataMongoDBConnection jp = MasterDataService.Connections[Region.JP];
            var ce = jp.GetCraftEssenceByCollectionNo(id);
            var title = $"{ce.MstSvt.CollectionNo}. {ce.MstSvt.Name} (`{ce.MstSvt.ID}`)";
            var description = $"Cost : {ce.MstSvt.Cost}";
            
            var embeds = new List<LocalEmbed>
            {
                new()
                {
                    Title = title,
                    Description = description,
                    Fields = ce.BaseSkills.SelectMany(skill => new SkillRenderer(skill.MstSkill, jp, skill).Prepare(TraitService).Fields)
                        .ToList()
                }
            };

            if (ce.MLBSkills.Length != 0)
                embeds.Add(
                    new LocalEmbed
                    {
                        Title = title,
                        Description = description,
                        Fields = ce.MLBSkills.SelectMany(skill => new SkillRenderer(skill.MstSkill, jp, skill).Prepare(TraitService).Fields)
                            .ToList()
                    });

            return View(new CEView(embeds));
        }
    }
}
