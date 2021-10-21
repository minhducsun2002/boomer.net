using System.Collections.Concurrent;
using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        private readonly ConcurrentDictionary<int, MstClass> classCache = new();

        public MstClass? ResolveClass(int classId, bool reload = false)
        {
            if (!reload && buffCache.ContainsKey(classId))
            {
                return classCache[classId];
            }

            var obj = MstClass.FindSync(Builders<MstClass>.Filter.Eq("id", classId)).FirstOrDefault();
            if (obj != null)
            {
                classCache[classId] = obj;
            }

            return obj;
        }
    }
}