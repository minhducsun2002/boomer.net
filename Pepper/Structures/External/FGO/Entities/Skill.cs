using System.Collections.Generic;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Entities
{
    public class Skill
    {
        internal Skill(MstSkill skill) { MstSkill = skill; }
        public readonly MstSkill MstSkill;
        public Dictionary<MstFunc, DataVal[]> Invocations = new();
    }
}