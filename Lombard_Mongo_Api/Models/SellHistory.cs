using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.AspNetCore.Components.Web;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;

namespace Lombard_Mongo_Api.Models
{
    public class SellHistory : IDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string Id { get; set; } = string.Empty;

        [BsonElement("_idUser")]
        public string _idUser { get; set; } = string.Empty;

        [BsonElement("category")]
        public string category { get; set; } = string.Empty;

        [BsonElement("Brand")]
        public string brand { get; set; } = string.Empty;

        [BsonElement("description")]
        public string description { get; set; } = string.Empty;

        [BsonElement("Status")]
        public string status { get; set; } = string.Empty;
    }
}