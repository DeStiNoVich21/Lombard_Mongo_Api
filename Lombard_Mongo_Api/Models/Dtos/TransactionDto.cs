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
    public class TransactionDto
    {

        [BsonElement("_idUser")]
        public string _idUser { get; set; } = string.Empty;

        [BsonElement("_idProduct")]
        public string _idProduct { get; set; } = string.Empty;

        [BsonElement("Status")]
        public string status { get; set; } = string.Empty;

    }
}