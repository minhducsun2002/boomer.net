using System.Collections.Concurrent;
using System.Linq;
using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        private readonly ConcurrentDictionary<int, string> itemNamesCache = new();
        public string? GetItemName(int itemId, bool reload = false)
        {
            if (!reload && itemNamesCache.ContainsKey(itemId)) return itemNamesCache[itemId];
            
            var item = MstItem.FindSync(Builders<MstItem>.Filter.Eq("id", itemId)).FirstOrDefault();
            if (item != default) itemNamesCache[itemId] = item.Name;
            return item?.Name;

        }        
    }
}