
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Lombard_Mongo_Api.MongoRepository.GenericRepository
{
    public class MongoRepository<TDocument> : IMongoRepository<TDocument> where TDocument : IDocument
    {
        private readonly IMongoCollection<TDocument> _collection;
        private readonly IMongoSettings _settings;
        public MongoRepository(IMongoSettings settings)
        {
            var database = new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));

            _settings = settings;
        }

        private protected string GetCollectionName(Type documentType)
        {
            try
            {
                var mongoCollectionAttribute = documentType.GetCustomAttributes(typeof(MongoCollectionAttribute), true)
                                            .FirstOrDefault() as MongoCollectionAttribute;

                if (mongoCollectionAttribute != null)
                {
                    return mongoCollectionAttribute.CollectionName;
                }
                else
                {
                    // Если атрибут не указан, возвращаем стандартное имя коллекции на основе имени типа TDocument
                    return typeof(TDocument).Name;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IQueryable<TDocument> AsQueryable()
        {
            try
            {
                return _collection.AsQueryable();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void InsertOne(TDocument document)
        {

            try
            {
                _collection.InsertOne(document);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void InsertMany(ICollection<TDocument> documents)
        {
            try
            {
                _collection.InsertMany(documents);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public TDocument FindById(string id)
        {
            try
            {
                var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
                return _collection.Find(filter).FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression)
        {
            try
            {
                return _collection.Find(filterExpression).FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void ReplaceOne(TDocument document)
        {
            try
            {
                var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
                _collection.ReplaceOne(filter, document);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void DeleteById(string id)
        {
            try
            {
                var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
                _collection.DeleteOne(filter);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void DeleteOne(Expression<Func<TDocument, bool>> filterExpression)
        {
            try
            {
                _collection.DeleteOne(filterExpression);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void DeleteMany(Expression<Func<TDocument, bool>> filterExpression)
        {
            try
            {
                _collection.DeleteMany(filterExpression);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
