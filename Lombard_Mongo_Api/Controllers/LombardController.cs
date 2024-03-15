﻿using Lombard_Mongo_Api.Models;
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

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllLombards()
        {
            try
            {

                var user = _LombardsRepository.AsQueryable().ToList();
                
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding Lombard");
                return StatusCode(500, $"An error has occurred: {ex.Message}");
            }
        }


        [HttpPost("addLombard")]
        public async Task<ActionResult> AddLombard(pointLombardDto addLombard)
        {
            try
            {

                var user = await _LombardsRepository.FindOne(p=> p.lombard_name == addLombard.name);
                if (user != null)
                {
                    return BadRequest("This username already exist, please choose another one");
                }
                Lombards lombard = new Lombards
                {
                    Id = "",
                    lombard_name = addLombard.name,
                    address = addLombard.address,
                    number = addLombard.number,
                    deleted = false
                    
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
                
                _logger.LogInformation($"Lombard Retrieved: {lombard.lombard_name}");
                return Ok(lombard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding Lombard");
                return StatusCode(500, $"An error has occurred: {ex.Message}");
            }
        }
        [HttpGet("Name")]
        public async Task<IActionResult> GetLombardByName(string name)
        {
            try
            {

                Lombards lombard = await _LombardsRepository.FindOne(p => p.lombard_name ==name);
                if (lombard == null)
                {
                    return NotFound();
                }

                _logger.LogInformation($"Lombard Retrieved: {lombard.lombard_name}");
                return Ok(lombard);
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

        [HttpDelete("Name")]
        public async Task<IActionResult> DeleteLombardByName(string name)
        {
            try
            {
                Lombards lombard = await _LombardsRepository.FindOne(p => p.lombard_name == name);
                if (lombard == null)
                {
                    return NotFound();
                }
                lombard.deleted = true; // Устанавливаем флаг удаления
                _LombardsRepository.ReplaceOne(lombard); // Заменяем документ в БД

                _logger.LogInformation($"Lombard has been soft deleted: {name}");
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
                var deletedLombards = _LombardsRepository.AsQueryable().Where(l => l.deleted == true).ToList();
               
                
                _logger.LogInformation($"Deleted Lombard's listing has been retrieved.");
                return Ok(deletedLombards);
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
                var lombards = _LombardsRepository.AsQueryable().Where(l => l.deleted==false).ToList();
     
              
                _logger.LogInformation($"Active Lombard's listing has been retrieved.");
                return Ok(lombards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving active Lombards");
                return StatusCode(500, $"An error has occurred: {ex.Message}");
            }
        }
    }
}
