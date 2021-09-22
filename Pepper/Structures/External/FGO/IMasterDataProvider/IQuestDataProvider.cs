using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public interface IQuestDataProvider
    {
        public MstQuest? ResolveQuest(int questId);
        public MstQuestPhase[] ListQuestPhases(int questId);
        public MstQuest[] ListQuestsByQuestType(FgoExportedConstants.QuestEntity.enType questType);
        public MstEvent GetEventById(int eventId);
    }
}