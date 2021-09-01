using System.Collections.Generic;
using System.Linq;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;

namespace Pepper.Structures.External.FGO.Entities
{
    public class TreasureDevice
    {
        private class FunctionComparer : IEqualityComparer<MstFunc>
        {
            public bool Equals(MstFunc? x, MstFunc? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x?.GetType() != y?.GetType()) return false;
                return x?.ID == y?.ID;
            }

            public int GetHashCode(MstFunc obj) => obj.ID;
            public static readonly FunctionComparer Instance = new();
        }

        public TreasureDevice(
            MstTreasureDevice mstTreasureDevice,
            MstTreasureDeviceLv[] levels,
            IEnumerable<MstFunc> resolvedFunctions
        )
        {
            var funcTable = resolvedFunctions.Distinct(FunctionComparer.Instance).ToDictionary(func => func.ID, func => func);
            
            MstTreasureDevice = mstTreasureDevice;
            Levels = levels;
            Functions = levels[0].FuncId.Select(function => funcTable[function]).ToArray();

            FuncToLevelsWithOvercharges = Functions.Select((function, index) =>
                {
                    var levels = Levels.Select(level =>
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
                            .Select(overcharge => DataValParser.Parse(overcharge, function.Type)).ToArray();

                        return (level, parsedAssociatedOvercharge);
                    }).ToArray();

                    return new KeyValuePair<MstFunc, (MstTreasureDeviceLv, DataVal[])[]>(function, levels);
                })
                .ToList();
        }

        public readonly List<KeyValuePair<MstFunc, (MstTreasureDeviceLv, DataVal[])[]>> FuncToLevelsWithOvercharges;
        public readonly MstFunc[] Functions;
        public readonly MstTreasureDevice MstTreasureDevice;
        public readonly MstTreasureDeviceLv[] Levels;

    }
}