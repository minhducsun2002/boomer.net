using System.Collections.Generic;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Entities
{
    public class TreasureDevice
    {
        internal TreasureDevice(
            MstTreasureDevice mstTreasureDevice,
            MstTreasureDeviceLv[] levels,
            Dictionary<MstTreasureDeviceLv, List<(MstFunc, DataVal[])>> levelToFuncWithOvercharges
        )
        {
            MstTreasureDevice = mstTreasureDevice;
            Levels = levels;
            LevelToFuncWithOvercharges = levelToFuncWithOvercharges;
        }

        public readonly Dictionary<MstTreasureDeviceLv, List<(MstFunc, DataVal[])>> LevelToFuncWithOvercharges;
        public readonly MstTreasureDevice MstTreasureDevice;
        public readonly MstTreasureDeviceLv[] Levels;

    }
}