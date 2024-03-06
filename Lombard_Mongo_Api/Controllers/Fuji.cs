using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lombard_Mongo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class Fuji : ControllerBase
    {
        private readonly IMongoRepository<Products> _dbRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Fuji> _logger;
        public Fuji(IConfiguration configuration, IMongoRepository<Products> dbRepository, ILogger<Fuji> logger)
        {
            _configuration = configuration;
            _dbRepository = dbRepository;
            _logger = logger;
        }
    }
}
