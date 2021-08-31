namespace Pepper.Structures.External.FGO
{
    public interface ITraitNameProvider
    {
        public string GetTrait(long traitId, bool fallbackToEmpty = false);
    }
}