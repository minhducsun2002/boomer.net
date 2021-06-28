using System.Collections.Concurrent;
using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        private readonly ConcurrentDictionary<int, MstBuff> buffCache = new();

        public MstBuff? ResolveBuff(int id, bool reload = false)
        {
            if (!reload && buffCache.ContainsKey(id)) return buffCache[id];
            
            var obj = MstBuff.FindSync(Builders<MstBuff>.Filter.Eq("id", id)).FirstOrDefault();
            if (obj != null) buffCache[id] = obj;
            return obj;
        }
    }
}