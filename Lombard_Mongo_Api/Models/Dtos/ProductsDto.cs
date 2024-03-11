using MongoDB.Bson.Serialization.Attributes;
namespace Lombard_Mongo_Api.Models.Dtos
{
    public class ProductsDto
    {
        public string name { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int price { get; set; }
        public string status { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        // Добавлено свойство brand
        public string brand { get; set; } = string.Empty;
    }
}
