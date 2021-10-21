using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        public MstSvtSkill[] GetServantSkillAssociationByServantId(int svtId)
            => MstSvtSkill.FindSync(Builders<MstSvtSkill>.Filter.Eq("svtId", svtId)).ToList().ToArray();

        public MstSvtSkill[] GetServantSkillAssociationBySkillId(int skillId)
            => MstSvtSkill.FindSync(Builders<MstSvtSkill>.Filter.Eq("skillId", skillId)).ToList().ToArray();
    }
}