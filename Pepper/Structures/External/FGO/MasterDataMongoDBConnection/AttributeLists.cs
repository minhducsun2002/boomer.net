using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        private int[] attributesCache = Array.Empty<int>();
        
        public int[] GetAttributeLists(bool reload = false)
        {
            if (attributesCache.Length != 0 && !reload) return attributesCache;
            
            const string stage = @"{ atk: { $addToSet: ""$atkAttri"" }, def: { $addToSet: ""$defAttri"" }, _id: null }";
            var attribs = MstAttriRelation.Aggregate().Group(BsonDocument.Parse(stage)).First()!;
            
            return attributesCache = attribs["atk"].AsBsonArray
                .Concat(attribs["def"].AsBsonArray).Select(value => (int) value).Distinct().Select(attrib => attrib + 199).ToArray();
        }
    }
}