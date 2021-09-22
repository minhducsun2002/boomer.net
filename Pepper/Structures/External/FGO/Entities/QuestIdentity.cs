namespace Pepper.Structures.External.FGO.Entities
{
    public class QuestIdentity
    {
        public int QuestId;
        public static implicit operator int(QuestIdentity quest) => quest.QuestId;
    }
}