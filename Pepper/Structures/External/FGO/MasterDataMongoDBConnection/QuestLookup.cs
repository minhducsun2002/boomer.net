using FgoExportedConstants;
using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        public MstQuest? ResolveQuest(int questId) =>
            MstQuest.FindSync(Builders<MstQuest>.Filter.Eq("id", questId)).FirstOrDefault();

        public MstQuestPhase[] ListQuestPhases(int questId) =>
            MstQuestPhase.FindSync(Builders<MstQuestPhase>.Filter.Eq("questId", questId)).ToList().ToArray();

        public MstQuest[] ListQuestsByQuestType(QuestEntity.enType questType) =>
            MstQuest.FindSync(Builders<MstQuest>.Filter.Eq("type", (int) questType)).ToList().ToArray();
    }
}