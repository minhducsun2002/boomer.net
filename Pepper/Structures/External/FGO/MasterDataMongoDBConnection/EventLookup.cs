using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        public MstEvent GetEventById(int eventId) => MstEvent.FindSync(Builders<MstEvent>.Filter.Eq("id", eventId)).First();
    }
}