using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
        // Убрано поле brand
        [BsonElement("imageFileName")]
        public string ImageFileName { get; set; } // Имя файла изображения
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