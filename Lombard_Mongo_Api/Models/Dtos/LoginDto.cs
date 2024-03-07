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
    public class LoginDto
    {
        [BsonElement("username")]
        public string username { get; set; } = string.Empty;
        [BsonElement("password")]
        public string password { get; set; } = string.Empty;
    }
}
