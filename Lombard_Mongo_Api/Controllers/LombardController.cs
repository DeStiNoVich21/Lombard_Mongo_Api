using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Lombard_Mongo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LombardController : ControllerBase
    {
        private readonly IMongoRepository<Lombards> _dbRepository;
        private readonly IConfiguration _configuration;
        public LombardController(IConfiguration configuration, IMongoRepository<Lombards> dbRepository)
        {
            _configuration = configuration;
            _dbRepository = dbRepository;
        }
        [HttpPost("addLombard")]
        public ActionResult AddLombard(addLombardDto addLombard)
        {
            try
            {
                string lombardName = "LombNet." + addLombard.Address;
                Lombards lombard = new Lombards
                {
                    lombard_name = lombardName,
                    address = addLombard.Address,
                    number = addLombard.Number,
                    description = addLombard.Description
                };
                _dbRepository.InsertOne(lombard);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Произошла ошибка: {ex.Message}");
            }
        }
    }
}
