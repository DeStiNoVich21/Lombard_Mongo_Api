using MongoDB.Driver;
using System.Linq.Expressions;
namespace Lombard_Mongo_Api.MongoRepository.GenericRepository
{
    public interface IMongoRepository<TDocument> where TDocument : IDocument
    {
        IQueryable<TDocument> AsQueryable();
        void InsertOne (TDocument document);
        void InsertMany (ICollection<TDocument> documents);
        Task<TDocument> FindById(string id);
        Task<TDocument> FindOne(Expression<Func<TDocument, bool>> filterExpression);
        void ReplaceOne(TDocument document);
        void DeleteById(string id);
        void DeleteOne(Expression<Func<TDocument, bool>> filterExpression);
        void DeleteMany(Expression<Func<TDocument, bool>> filterExpression);
        Task GetByIdAsync(string id);
        Task<List<TDocument>> FindAsync(FilterDefinition<TDocument> filter);
        Task<List<TDocument>> GetAllAsync();
    }
}