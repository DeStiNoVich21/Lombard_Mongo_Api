using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.AspNetCore.Components.Web;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
namespace Lombard_Mongo_Api.Models
{
    public class Products : IDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [BsonElement("name")]
        public string name { get; set; } = string.Empty;
        [BsonElement("category")]
        public string category { get; set; } = string.Empty;
        [BsonElement("image")]
        public string image { get; set; } = string.Empty;
        [BsonElement("description")]
        public string description { get; set; } = string.Empty;
        [BsonElement("price")]
        public int price { get; set; }
        [BsonElement("status")]
        public string status { get; set; } = string.Empty;
        [BsonElement("isdeleted")]
        public bool IsDeleted { get; set; }
    }
}