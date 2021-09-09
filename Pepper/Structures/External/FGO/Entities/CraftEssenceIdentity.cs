namespace Pepper.Structures.External.FGO.Entities
{
    public class CraftEssenceIdentity
    {
        public int CraftEssenceId;
        public static implicit operator int(CraftEssenceIdentity ce) => ce.CraftEssenceId;
    }
}