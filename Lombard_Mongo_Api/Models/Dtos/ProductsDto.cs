using MongoDB.Bson.Serialization.Attributes;
namespace Lombard_Mongo_Api.Models.Dtos
{
    public class ProductsDto
    {
        public string Id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public string image { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int price { get; set; }
        public string status { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }
}
