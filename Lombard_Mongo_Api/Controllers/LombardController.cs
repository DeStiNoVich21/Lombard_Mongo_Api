using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
namespace Lombard_Mongo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "admin")] 
    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize] 
    public class LombardController : ControllerBase
    {
        private readonly IMongoRepository<Lombards> _LombardsRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LombardController> _logger;
        public LombardController(IConfiguration configuration, IMongoRepository<Lombards> dbRepository, ILogger<LombardController> logger)
        {
            _configuration = configuration;
            _LombardsRepository = dbRepository;
            _logger = logger;
        }
        [HttpPost("addLombard")]
        public async Task<ActionResult> AddLombard(pointLombardDto addLombard)
        {
            try
            {
               
                string lombardName = "LombNet." + addLombard.address;
                Lombards lombard = new Lombards
                {
                    lombard_name = lombardName,
                    address = addLombard.address,
                    number = addLombard.number
                };
                _LombardsRepository.InsertOne(lombard);
                _logger.LogInformation($"Lombard has been added: {lombard.lombard_name}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding Lombard");
                return StatusCode(500, $"An error has occurred: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLombardById(string id)
        {
            try
            {
               
                Lombards lombard = await _LombardsRepository.FindById(id);
                if (lombard == null)
                {
                    return NotFound();
                }
                var lombardDto = new pointLombardDto
                {
                    Id = lombard.Id,
                    name = lombard.lombard_name,
                    address = lombard.address,
                    number = lombard.number
                };
                _logger.LogInformation($"Lombard Retrieved: {lombardDto.name}");
                return Ok(lombardDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding Lombard");
                return StatusCode(500, $"An error has occurred: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLombardById(string id)
        {
            try
            {
                Lombards lombard = await _LombardsRepository.FindById(id);
                if (lombard == null)
                {
                    return NotFound();
                }
                lombard.deleted = true; // Устанавливаем флаг удаления
                _LombardsRepository.ReplaceOne(lombard); // Заменяем документ в БД

                _logger.LogInformation($"Lombard has been soft deleted: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while soft deleting Lombard");
                return StatusCode(500, $"An error has occurred: {ex.Message}");
            }
        }
        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeletedLombards()
        {
            try
            {
                var deletedLombards = _LombardsRepository.AsQueryable().Where(l => l.deleted).ToList();
                var deletedLombardDtos = new List<pointLombardDto>();
                foreach (var deletedLombard in deletedLombards)
                {
                    var deletedLombardDto = new pointLombardDto
                    {
                        Id = deletedLombard.Id,
                        name = deletedLombard.lombard_name,
                        address = deletedLombard.address,
                        number = deletedLombard.number
                    };
                    deletedLombardDtos.Add(deletedLombardDto);
                }
                _logger.LogInformation($"Deleted Lombard's listing has been retrieved.");
                return Ok(deletedLombardDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving deleted Lombards");
                return StatusCode(500, $"An error has occurred: {ex.Message}");
            }
        }
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveLombards()
        {
            try
            {
                var lombards = _LombardsRepository.AsQueryable().Where(l => !l.deleted).ToList();
                var lombardDtos = new List<pointLombardDto>();
                foreach (var lombard in lombards)
                {
                    var lombardDto = new pointLombardDto
                    {
                        Id = lombard.Id,
                        name = lombard.lombard_name,
                        address = lombard.address,
                        number = lombard.number
                    };
                    lombardDtos.Add(lombardDto);
                }
                _logger.LogInformation($"Active Lombard's listing has been retrieved.");
                return Ok(lombardDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving active Lombards");
                return StatusCode(500, $"An error has occurred: {ex.Message}");
            }
        }
    }
}
