using System.Collections.Concurrent;
using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        private readonly ConcurrentDictionary<int, MstSpot> spots = new();
        private readonly ConcurrentDictionary<int, MstWar> wars = new();

        public MstSpot? ResolveSpot(int spotId, bool reload = false)
        {
            // spots are cheap to fully cache, let's fill it up if we haven't done so.
            if (spots.IsEmpty)
            {
                foreach (var spotObj in MstSpot.FindSync(Builders<MstSpot>.Filter.Empty).ToList())
                {
                    spots[spotObj.ID] = spotObj;
                }
            }
            
            if (spots.TryGetValue(spotId, out var @return) && !reload) return @return;

            var spot = MstSpot.FindSync(Builders<MstSpot>.Filter.Eq("id", spotId)).FirstOrDefault();
            return spot != default ? spots[spotId] = spot : default;
        }

        public MstWar? ResolveWar(int warId, bool reload = false)
        {
            if (wars.TryGetValue(warId, out var @return) && !reload) return @return;

            var war = MstWar.FindSync(Builders<MstWar>.Filter.Eq("id", warId)).FirstOrDefault();
            return war != default ? wars[warId] = war : default;
        }
    }
}