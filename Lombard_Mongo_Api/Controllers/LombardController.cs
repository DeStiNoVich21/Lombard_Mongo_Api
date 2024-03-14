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
                    number = addLombard.number,
                    description = addLombard.description
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
        [HttpGet]
        public async Task<IActionResult> GetAllLombards()
        {
            try
            {
                
                var lombards = _LombardsRepository.AsQueryable().ToList();
                var lombardDtos = new List<pointLombardDto>();
                foreach (var lombard in lombards)
                {
                    var lombardDto = new pointLombardDto
                    {
                        Id = lombard.Id,
                        name = lombard.lombard_name,
                        address = lombard.address,
                        number = lombard.number,
                        description = lombard.description
                    };
                    lombardDtos.Add(lombardDto);
                }
                _logger.LogInformation($"Lombard's listing has been retrieved.");
                return Ok(lombardDtos);
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
                    number = lombard.number,
                    description = lombard.description
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
                _LombardsRepository.DeleteById(id);
                _logger.LogInformation($"Lombard has been removed: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding Lombard");
                return StatusCode(500, $"An error has occurred: {ex.Message}");
            }
        }
    }
}
