using System.Threading.Tasks;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.Commands.Result;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;
using Qmmands;

namespace Pepper.Commands.FGO
{
    public class Skill : FGOCommand
    {
        [Command("skill")]
        public async Task<EmbedResult> Exec(int id = 5450)
        {
            var jp = MasterDataService.Connections[Region.JP];
            var skill = jp.MstSkill.FindSync(Builders<MstSkill>.Filter.Eq("id", id)).First();
            var rendered = new SkillRenderer(skill, jp).Prepare(TraitService);
            return new EmbedResult
            {
                DefaultEmbed = rendered.Build()
            };
        }
    }
}