using System;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Entities
{
    public class CraftEssence
    {
        public CraftEssence(MstSvt mstSvt) { MstSvt = mstSvt; }
        public MstSvt MstSvt;
        public Skill[] BaseSkills = Array.Empty<Skill>();
        public Skill[] MLBSkills = Array.Empty<Skill>();
    }
}