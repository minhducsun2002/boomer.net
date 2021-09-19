using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public interface IItemDataProvider
    {
        public MstItem[] GetItemsByIndividuality(int individualty);
        public string? GetItemName(int itemId, bool reload = false);
    }
}