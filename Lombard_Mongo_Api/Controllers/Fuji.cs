﻿using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
namespace Lombard_Mongo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("_myAllowSpecificOrigins")]
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
        [HttpPost("addProduct")]
        public async Task<ActionResult> AddProduct([FromBody] ProductsDto productDto)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }
                var product = new Products
                {
                    name = productDto.name,
                    category = productDto.category,
                    image = productDto.image,
                    description = productDto.description,
                    price = productDto.price,
                    status = productDto.status,
                    IsDeleted = productDto.IsDeleted
                };
                _dbRepository.InsertOne(product);
                _logger.LogInformation($"製品が追加されました: {product.name}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "製品の追加中にエラーが発生しました");
                return StatusCode(500, $"エラーが発生しました: {ex.Message}");
            }
        }
        /*[HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }

                var categories = await _dbRepository
                    .Select(p => p.Category)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation($"カテゴリのリストが取得されました");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "カテゴリのリストを取得中にエラーが発生しました");
                return StatusCode(500, $"内部サーバーエラー: {ex.Message}");
            }
        }*/
    }
}
