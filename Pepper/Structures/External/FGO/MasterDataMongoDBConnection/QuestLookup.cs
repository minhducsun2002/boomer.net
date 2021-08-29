using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        public MstQuest? ResolveQuest(int questId) => 
            MstQuest.FindSync(Builders<MstQuest>.Filter.Eq("id", questId)).FirstOrDefault();
    }
}