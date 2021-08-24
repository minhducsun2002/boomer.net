using System.Collections.Generic;
using System.Linq;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;

namespace Pepper.Structures.External.FGO.Entities
{
    public class TreasureDevice
    {
        internal TreasureDevice(
            MstTreasureDevice mstTreasureDevice,
            MstTreasureDeviceLv[] levels,
            IEnumerable<MstFunc> resolvedFunctions
        )
        {
            var funcTable = resolvedFunctions.ToDictionary(func => func.ID, func => func);
            
            MstTreasureDevice = mstTreasureDevice;
            Levels = levels;
            functions = levels[0].FuncId.Select(function => funcTable[function]).ToArray();

            FuncToLevelsWithOvercharges = functions.Select((function, index) =>
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

                    return (function, levels);
                })
                .ToDictionary(pair => pair.function, pair => pair.levels);
        }

        public readonly Dictionary<MstFunc, (MstTreasureDeviceLv, DataVal[])[]> FuncToLevelsWithOvercharges;
        public readonly MstFunc[] functions;
        public readonly MstTreasureDevice MstTreasureDevice;
        public readonly MstTreasureDeviceLv[] Levels;

    }
}