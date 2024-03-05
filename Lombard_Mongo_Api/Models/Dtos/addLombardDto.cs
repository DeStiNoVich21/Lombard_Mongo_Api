using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.AspNetCore.Components.Web;
using System.Reflection.Metadata;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;

namespace Lombard_Mongo_Api.Models.Dtos
{
    public class addLombardDto
    {
        [BsonElement("address")]
        public string? Address { get; set; } = string.Empty;
        [BsonElement("number")]
        public string? Number { get; set; } = string.Empty;
        [BsonElement("description")]
        public string? Description { get; set; } = string.Empty;
    }
}
