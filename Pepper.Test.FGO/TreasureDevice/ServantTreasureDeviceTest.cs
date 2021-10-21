using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FgoExportedConstants;
using MongoDB.Bson.Serialization;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.MasterData;
using Xunit;
using Xunit.Sdk;

namespace Pepper.Test.FGO.TreasureDevice
{
    internal class TraitProvider : ITraitNameProvider
    {
        public string GetTrait(int traitId, bool fallbackToEmpty = false)
        {
            return traitId.ToString();
        }

        public static readonly TraitProvider Instance = new();
    }

    public class NPDataProvider : ITreasureDeviceDataProvider, IQuestDataProvider, IItemDataProvider, IBaseObjectsDataProvider
    {
        public readonly Dictionary<int, MstSvt> ServantEntitiesById;
        private readonly Dictionary<int, List<MstSvtTreasureDevice>> cachedServantTreasureDevicesById;
        private readonly Dictionary<int, MstTreasureDevice> treasureDevicesById;
        private readonly Dictionary<int, MstTreasureDeviceLv[]> mstTreasureDeviceLvById;
        private readonly Dictionary<int, MstFunc> functions;
        private readonly Dictionary<int, MstBuff> buffs;
        private readonly Dictionary<int, MstQuest> quests;

        public NPDataProvider(string path)
        {
            ServantEntitiesById = ResolveArray<MstSvt>(path, Names.MstSvt)
                .Where(
                    entity => entity.Type is SvtType.Type.HEROINE or SvtType.Type.NORMAL or SvtType.Type.ENEMY_COLLECTION_DETAIL
                )
                .ToDictionary(entity => entity.BaseSvtId, entity => entity);

            var validTreasureDeviceIds = new HashSet<int>();
            cachedServantTreasureDevicesById = ResolveArray<MstSvtTreasureDevice>(path, Names.MstSvtTreasureDevice)
                .Where(map => ServantEntitiesById.ContainsKey(map.SvtId))
                .Select(map =>
                {
                    validTreasureDeviceIds.Add(map.TreasureDeviceId);
                    return map;
                })
                .GroupBy(map => map.SvtId)
                .ToDictionary(map => map.Key, map => map.ToList());

            buffs = ResolveArray<MstBuff>(path, Names.MstBuff).ToDictionary(buff => buff.ID, func => func);
            functions = ResolveArray<MstFunc>(path, Names.MstFunc).ToDictionary(func => func.ID, func => func);
            mstTreasureDeviceLvById = ResolveArray<MstTreasureDeviceLv>(path, Names.MstTreasureDeviceLv)
                .Where(lv => validTreasureDeviceIds.Contains(lv.TreaureDeviceId))
                .GroupBy(lv => lv.TreaureDeviceId)
                .ToDictionary(lv => lv.Key, lv => lv.ToArray());

            treasureDevicesById = ResolveArray<MstTreasureDevice>(path, Names.MstTreasureDevice)
                .Where(td => validTreasureDeviceIds.Contains(td.ID))
                .ToDictionary(
                    td => td.ID,
                    td => td
                );

            quests = ResolveArray<MstQuest>(path, Names.MstQuest).ToDictionary(quest => quest.ID, quest => quest);
        }

        private static T[] ResolveArray<T>(string path, string name) where T : MasterDataEntity =>
            BsonSerializer.Deserialize<T[]>(File.ReadAllText(Path.Combine(path, name + ".json")));

        public MstSvt GetServantEntityById(int id) => ServantEntitiesById[id];

        public List<MstSvtTreasureDevice> GetCachedServantTreasureDevices(int servantId, bool reload = false)
            => cachedServantTreasureDevicesById.TryGetValue(servantId, out var @out) ? @out : new();

        public Structures.External.FGO.Entities.TreasureDevice GetTreasureDevice(int treasureDeviceId)
        {
            var td = treasureDevicesById[treasureDeviceId];
            var _ = new Structures.External.FGO.Entities.TreasureDevice(
                td,
                mstTreasureDeviceLvById[td.ID],
                mstTreasureDeviceLvById[td.ID][0].FuncId.Select(func => functions[func])
            );
            return _;
        }

        public MstTreasureDevice GetTreasureDeviceEntity(int treasureDeviceId) => treasureDevicesById[treasureDeviceId];
        public MstQuest ResolveQuest(int questId) => quests[questId];
        public MstQuestPhase[] ListQuestPhases(int questId) { throw new NotImplementedException(); }
        public MstQuest[] ListQuestsByQuestType(QuestEntity.enType questType) { throw new NotImplementedException(); }

        public MstBuff ResolveBuffAndCache(int id, bool reload = false) => buffs[id];
        public MstClass ResolveClass(int classId, bool reload = false) { throw new NotImplementedException(); }
        public MstEvent GetEventById(int eventId) { throw new NotImplementedException(); }
        public MstItem[] GetItemsByIndividuality(int individualty) { throw new NotImplementedException(); }
        public string GetItemName(int itemId, bool reload = false) { throw new NotImplementedException(); }
    }

    internal class NPMasterProvideAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var path = Path.Combine(Names.BasePath, Names.Versions[0]);
            var provider = new NPDataProvider(path);
            return provider.ServantEntitiesById.Keys.Select(key => new object[] { key, provider });
        }
    }

    public class ServantTreasureDeviceTest
    {
        [Theory]
        [NPMasterProvide]
        public void PrepareNP(int servant, NPDataProvider masterDataProvider)
        {
            var servantName = masterDataProvider.GetServantEntityById(servant)!.Name;
            var pages = Commands.FGO.TreasureDevice.SerializePages(
                servant,
                servantName,
                masterDataProvider,
                masterDataProvider, TraitProvider.Instance
            );

            foreach (var page in pages)
            {
                foreach (var embed in page.page.Embeds)
                {
                    embed.Validate();
                }
            }
        }
    }
}