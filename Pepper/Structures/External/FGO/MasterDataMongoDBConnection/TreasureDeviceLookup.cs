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
        public TreasureDevice GetTreasureDevice(int treasureDeviceId)
        {
            var mstTreasureDevice = MstTreasureDevice
                .Find(Builders<MstTreasureDevice>.Filter.Eq("id", treasureDeviceId))
                .Limit(1)
                .First();

            var levels = MstTreasureDeviceLv
                .Find(Builders<MstTreasureDeviceLv>.Filter.Eq("treaureDeviceId", treasureDeviceId))
                .SortBy(lv => lv.Level)
                .ToList();

            var functions = levels[0].FuncId.Select(function => ResolveFunc(function)!);
            
            return new TreasureDevice(mstTreasureDevice, levels.ToArray(), functions);
        }


        private readonly Dictionary<int, List<MstSvtTreasureDevice>> svtTreasureDeviceCache = new();

        // public List<MstSvtTreasureDevice> GetServantTreasureDevices(MstSvt mstSvt, bool reload = false) => GetServantTreasureDevices(mstSvt.ID, reload);
        // public List<MstSvtTreasureDevice> GetServantTreasureDevices(BaseServant servant, bool reload = false) => GetServantTreasureDevices(servant.ID, reload);
        public List<MstSvtTreasureDevice> GetServantTreasureDevices(int servantId, bool reload = false)
            => svtTreasureDeviceCache.TryGetValue(servantId, out var result)
                ? result
                : svtTreasureDeviceCache[servantId] = MstSvtTreasureDevice
                        .FindSync(Builders<MstSvtTreasureDevice>.Filter.Eq("svtId", servantId))
                    .ToList();
    }
}