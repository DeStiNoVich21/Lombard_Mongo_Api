using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; 
namespace Lombard_Mongo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LombardController : ControllerBase
    {
        private readonly IMongoRepository<Lombards> _dbRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LombardController> _logger; 
        public LombardController(IConfiguration configuration, IMongoRepository<Lombards> dbRepository, ILogger<LombardController> logger)
        {
            _configuration = configuration;
            _dbRepository = dbRepository;
            _logger = logger; 
        }
        [HttpPost("addLombard")]
        public ActionResult AddLombard(addLombardDto addLombard)
        {
            try
            {
                string lombardName = "LombNet." + addLombard.address;
                Lombards lombard = new Lombards
                {
                    lombard_name = lombardName,
                    address = addLombard.address,
                    number = addLombard.number,
                    description = addLombard.description
                };
                _dbRepository.InsertOne(lombard);
                _logger.LogInformation($"Lombard added: {lombard.lombard_name}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding Lombard"); // Log error
                return StatusCode(500, $"Произошла ошибка: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public IActionResult GetLombardById(string id)
        {
            try
            {
                Lombards lombard = _dbRepository.FindById(id);
                if (lombard == null)
                {
                    return NotFound();
                }
                var lombardDto = new addLombardDto
                {
                    Id = lombard.Id,
                    name = lombard.lombard_name,
                    address = lombard.address,
                    number = lombard.number,
                    description = lombard.description
                };
                _logger.LogInformation($"Lombard retrieved: {lombardDto.name}");

                return Ok(lombardDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting Lombard by Id"); 
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}