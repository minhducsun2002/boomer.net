namespace Pepper.Structures.External.FGO
{
    public interface ITraitNameProvider
    {
        public string GetTrait(int traitId, bool fallbackToEmpty = false);
    }
}