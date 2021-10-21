using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        public MstTreasureDevice? GetTreasureDeviceEntity(int treasureDeviceId)
            => MstTreasureDevice
                .Find(Builders<MstTreasureDevice>.Filter.Eq("id", treasureDeviceId))
                .Limit(1)
                .FirstOrDefault();

        public TreasureDevice GetTreasureDevice(int treasureDeviceId)
        {
            var mstTreasureDevice = GetTreasureDeviceEntity(treasureDeviceId);

            var levels = MstTreasureDeviceLv
                .Find(Builders<MstTreasureDeviceLv>.Filter.Eq("treaureDeviceId", treasureDeviceId))
                .SortBy(lv => lv.Level)
                .ToList();

            var functions = levels[0].FuncId.Select(function => ResolveFunc(function)!);

            return new TreasureDevice(mstTreasureDevice!, levels.ToArray(), functions);
        }


        private readonly Dictionary<int, List<MstSvtTreasureDevice>> svtTreasureDeviceCache = new();

        public List<MstSvtTreasureDevice> GetCachedServantTreasureDevices(int servantId, bool reload = false)
            => svtTreasureDeviceCache.TryGetValue(servantId, out var result)
                ? result
                : svtTreasureDeviceCache[servantId] = MstSvtTreasureDevice
                        .FindSync(Builders<MstSvtTreasureDevice>.Filter.Eq("svtId", servantId))
                    .ToList();

        public MstTreasureDeviceLv GetNPGain(int svtId)
        {
            var mapping = MstSvtTreasureDevice.FindSync(
                Builders<MstSvtTreasureDevice>.Filter.And(
                    Builders<MstSvtTreasureDevice>.Filter.Eq("svtId", svtId),
                    Builders<MstSvtTreasureDevice>.Filter.Eq("num", 1)
                ),
                new FindOptions<MstSvtTreasureDevice> { Limit = 1 }
            ).First()!;
            return MstTreasureDeviceLv.FindSync(
                Builders<MstTreasureDeviceLv>.Filter.Eq("treaureDeviceId", mapping.TreasureDeviceId),
                new FindOptions<MstTreasureDeviceLv> { Limit = 1 }
            ).First()!;
        }
    }
}