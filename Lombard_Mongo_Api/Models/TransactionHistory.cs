using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.AspNetCore.Components.Web;

namespace Lombard_Mongo_Api.Models
{
    public class TransactionHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string _id { get; set; } = string.Empty;

        [BsonElement("_idUser")]
        public string _idUser { get; set; } = string.Empty;

        [BsonElement("_idProduct")]
        public string _idProduct { get; set; } = string.Empty;

        [BsonElement("Status")]
        public string status {  get; set; } = string.Empty;
    }
}
