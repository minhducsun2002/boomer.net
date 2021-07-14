using Disqord.Bot;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;
using Qmmands;

namespace Pepper.Commands.FGO
{
    public class Skill : FGOCommand
    {
        [Command("skill")]
        public DiscordCommandResult Exec(int id = 5450)
        {
            var jp = MasterDataService.Connections[Region.JP];
            var skill = jp.MstSkill.FindSync(Builders<MstSkill>.Filter.Eq("id", id)).First();
            var rendered = new SkillRenderer(skill, jp).Prepare(TraitService);
            return Reply(rendered);
        }
    }
}