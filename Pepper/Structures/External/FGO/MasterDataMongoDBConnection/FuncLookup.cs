using System.Collections.Concurrent;
using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        private readonly ConcurrentDictionary<int, MstFunc> funcCache = new();

        public MstFunc? ResolveFunc(int id, bool reload = false)
        {
            if (!reload && funcCache.ContainsKey(id)) return funcCache[id];
            
            var obj = MstFunc.FindSync(Builders<MstFunc>.Filter.Eq("id", id)).FirstOrDefault();
            if (obj != null) funcCache[id] = obj;
            return obj;

        }
    }
}