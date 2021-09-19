using System.Collections.Generic;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public interface ITreasureDeviceDataProvider
    {
        public List<MstSvtTreasureDevice> GetCachedServantTreasureDevices(int servantId, bool reload = false);
        public TreasureDevice GetTreasureDevice(int treasureDeviceId);
        public MstTreasureDevice? GetTreasureDeviceEntity(int treasureDeviceId);
        public MstTreasureDeviceLv GetNPGain(int svtId);
    }
}