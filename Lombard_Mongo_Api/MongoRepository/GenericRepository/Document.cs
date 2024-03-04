using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lombard_Mongo_Api.MongoRepository.GenericRepository
{
    public class Document : IDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    }
}
