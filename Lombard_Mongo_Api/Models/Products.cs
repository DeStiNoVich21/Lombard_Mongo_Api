using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.AspNetCore.Components.Web;

namespace Lombard_Mongo_Api.Models
{
    public class Products
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string _id { get; set; } = string.Empty;

        [BsonElement("image")]
        public string image { get; set; } = string.Empty;

        [BsonElement("description")]
        public string description { get; set; } = string.Empty;

        [BsonElement("category")]
        public string category {  get; set; } = string.Empty;

        [BsonElement("price")]
        public int price {  get; set; }

        [BsonElement("status")]
        public string status { get; set; } = string.Empty;

        [BsonElement("isdeleted")]
        public bool IsDeleted { get; set; }

    }
}
