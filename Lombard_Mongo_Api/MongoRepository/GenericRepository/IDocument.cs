using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Lombard_Mongo_Api.MongoRepository.GenericRepository
{
    public interface IDocument
    {
        string Id { get; set; }
    }
}
