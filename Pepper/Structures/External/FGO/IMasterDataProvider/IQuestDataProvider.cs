using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public interface IQuestDataProvider
    {
        public MstQuest? ResolveQuest(int questId);
        public MstEvent GetEventById(int eventId);
    }
}