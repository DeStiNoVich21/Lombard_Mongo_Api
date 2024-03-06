using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize] 
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
        public async Task<ActionResult> AddLombard(pointLombardDto addLombard)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }
                string lombardName = "LombNet." + addLombard.address;
                Lombards lombard = new Lombards
                {
                    lombard_name = lombardName,
                    address = addLombard.address,
                    number = addLombard.number,
                    description = addLombard.description
                };
                _dbRepository.InsertOne(lombard);
                _logger.LogInformation($"ロンバードが追加されました: {lombard.lombard_name}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ロンバードの追加中にエラーが発生しました");
                return StatusCode(500, $"エラーが発生しました: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllLombards()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }
                var lombards = _dbRepository.AsQueryable().ToList();
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
                _logger.LogInformation($"ロンバードのリストが取得されました");
                return Ok(lombardDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ロンバードのリストを取得中にエラーが発生しました");
                return StatusCode(500, $"内部サーバーエラー: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLombardById(string id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }
                Lombards lombard = await _dbRepository.FindById(id);
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
                _logger.LogInformation($"ロンバードが取得されました: {lombardDto.name}");
                return Ok(lombardDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IDでロンバードを取得中にエラーが発生しました");
                return StatusCode(500, $"内部サーバーエラー: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLombardById(string id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }
                Lombards lombard = await _dbRepository.FindById(id);
                if (lombard == null)
                {
                    return NotFound();
                }
                _dbRepository.DeleteById(id);
                _logger.LogInformation($"ロンバードが削除されました: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IDでロンバードを削除中にエラーが発生しました");
                return StatusCode(500, $"内部サーバーエラー: {ex.Message}");
            }
        }
    }
}
