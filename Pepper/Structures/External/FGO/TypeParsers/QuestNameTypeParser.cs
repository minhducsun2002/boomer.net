using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using FgoExportedConstants;
using FuzzySharp;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;
using Qmmands;
using Serilog;

namespace Pepper.Structures.External.FGO.TypeParsers
{
    internal class QuestWithSpotDetail
    {
        public QuestWithSpotDetail(MstQuest quest, MstSpot spot)
        {
            Quest = quest;
            Spot = spot;
        }
        public readonly MstQuest Quest;
        public readonly MstSpot Spot;
    }
    
    public class QuestNameTypeParser : DiscordTypeParser<QuestIdentity>
    {
        private static readonly ILogger Log = Serilog.Log.Logger.ForContext<QuestNameTypeParser>();
        internal readonly SearchableKeyedNamedEntityCollection<int, QuestWithSpotDetail> SearchableCollection;
        public QuestNameTypeParser(MasterDataService masterDataService)
        {
            var na = masterDataService.Connections[Region.NA];
            var quests = na.ListQuestsByQuestType(QuestEntity.enType.FREE);
            Log.Information($"Discovered {quests.Length} quests. Matching it into spots...");
            var filledQuests = quests.Select(quest => new QuestWithSpotDetail(quest, na.ResolveSpot(quest.SpotId)!));
            SearchableCollection = new SearchableKeyedNamedEntityCollection<int, QuestWithSpotDetail>(
                filledQuests.Select(quest =>
                    new NamedKeyedEntity<int, QuestWithSpotDetail>(
                        quest.Quest.ID, quest, quest.Quest.Name,
                        new[] { quest.Spot.Name })
                )
            );
        }


        public override ValueTask<TypeParserResult<QuestIdentity>> ParseAsync(Parameter parameter, string value, DiscordCommandContext context)
        {
            if (int.TryParse(value, out var questId)) return Success(new QuestIdentity { QuestId = questId });

            var search = SearchableCollection.FuzzySearch(value, aliasWeight: 1, scorer: Fuzz.PartialRatio);

            return Success(new QuestIdentity { QuestId = search.First().Key });
        }
    }
}