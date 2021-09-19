using System.Collections.Generic;
using FgoExportedConstants;

namespace Pepper.Structures.External.FGO.Renderer
{
    public partial class TypeNames
    {
        public static readonly Dictionary<QuestEntity.enType, string> QuestTypeNames = new()
        {
            { QuestEntity.enType.FREE, "Free" },
            { QuestEntity.enType.MAIN, "Main" },
            { QuestEntity.enType.EVENT, "Event" },
            { QuestEntity.enType.FRIENDSHIP, "Interlude" },
            { QuestEntity.enType.WAR_BOARD, "Warboard" },
            { QuestEntity.enType.HEROBALLAD, "Heroballad" },
        };
    }
}