using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.MongoDB
{
    public class Document
    {
        [BsonId] public ObjectId BsonDocumentId;
    }
}