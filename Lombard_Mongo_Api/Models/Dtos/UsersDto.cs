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
    public class UsersDto 
    {
        [BsonElement("username")]
        public string username { get; set; } = string.Empty;

        [BsonElement("password")]
        public string password { get; set; } = string.Empty;

        [BsonElement("email")]
        public string email { get; set; } = string.Empty;

        [BsonElement("number")]
        public string number { get; set; } = string.Empty;

    }
    public class UsersGetInfoDto
    {
        [BsonElement("username")]
        public string username { get; set; } = string.Empty;

        [BsonElement("_idLombard")]
        public string _idLombard { get; set; }

        [BsonElement("email")]
        public string email { get; set; } = string.Empty;

        [BsonElement("number")]
        public string number { get; set; } = string.Empty;

    }
    public class ModDto
    {
        [BsonElement("username")]
        public string username { get; set; } = string.Empty;

        [BsonElement("password")]
        public string password { get; set; } = string.Empty;

        [BsonElement("email")]
        public string email { get; set; } = string.Empty;

        [BsonElement("number")]
        public string number { get; set; } = string.Empty;

        [BsonElement("_idLombard")]
        public string _idLombard { get; set; }
    }
    public class ModUpdateDto
    {
        [BsonElement("username")]
        public string username { get; set; } = string.Empty;

        [BsonElement("email")]
        public string email { get; set; } = string.Empty;

        [BsonElement("number")]
        public string number { get; set; } = string.Empty;
    }
}