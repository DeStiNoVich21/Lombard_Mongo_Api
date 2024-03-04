using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.AspNetCore.Components.Web;

namespace Lombard_Mongo_Api.Models
{
    public class Lombards
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string _id { get; set; }
        [BsonElement("name")]
        public string lombard_name { get; set; } = "default_name";

        [BsonElement("address")]
        public string? address { get; set; } = string.Empty;

        [BsonElement("number")]
        public string? number { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? description { get; set; } = string.Empty;
        


       

    }
}