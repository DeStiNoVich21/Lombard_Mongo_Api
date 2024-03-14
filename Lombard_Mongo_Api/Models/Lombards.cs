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
    public class Lombards : IDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("name")]
        public string lombard_name { get; set; } = "LombNet";
        [BsonElement("address")]
        public string? address { get; set; } = string.Empty;
        [BsonElement("number")]
        public string? number { get; set; } = string.Empty;
        [BsonElement("deleted")]
        public bool deleted { get; set; } = false;
    }
}