namespace Lombard_Mongo_Api.MongoRepository.GenericRepository
{
    public interface IMongoSettings
    {
        string DatabaseName { get; set; }
        string ConnectionString { get; set; }
    }
}
