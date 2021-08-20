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

            var invocations = levels.ToDictionary(
                level => level,
                level => level.FuncId
                    .Select(function => ResolveFunc(function))
                    .Select((function, index) =>
                    {
                        var overchargeDataVal = new List<string>
                        {
                            level.Svals[index],
                            level.Svals2[index],
                            level.Svals3[index],
                            level.Svals4[index],
                            level.Svals5[index],
                        };

                        var parsedAssociatedOvercharge = overchargeDataVal
                            .Select(overcharge => DataValParser.Parse(overcharge, function!.Type));

                        return (function!, parsedAssociatedOvercharge.ToArray());
                    })
                    .ToList()
            );
            
            return new TreasureDevice(mstTreasureDevice, levels.ToArray(), invocations);
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