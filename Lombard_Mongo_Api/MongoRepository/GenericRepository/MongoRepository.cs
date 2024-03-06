
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

        public async void InsertOne(TDocument document)
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

        public async void InsertMany(ICollection<TDocument> documents)
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

        public async  Task<TDocument> FindById(string id)
        {
            try
            {
                var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
                return await _collection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TDocument>  FindOne(Expression<Func<TDocument, bool>> filterExpression)
        {
            try
            {
                return await _collection.Find(filterExpression).FirstOrDefaultAsync();
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

        public async void DeleteById(string id)
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

        public async void DeleteOne(Expression<Func<TDocument, bool>> filterExpression)
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

        public async void DeleteMany(Expression<Func<TDocument, bool>> filterExpression)
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

        public  async Task GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
