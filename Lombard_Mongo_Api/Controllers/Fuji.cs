﻿using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
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
                    return Unauthorized("User is not authenticated");
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
                _logger.LogInformation($"Product has been added: {product.name}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "sdfghjkl;sxdfghjkl;dfghjk");
                return StatusCode(500, $"An error has occurred.: {ex.Message}");
            }
        }
        [HttpGet("products")]
        public async Task<ActionResult<IEnumerable<Products>>> GetProducts()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }
                var products = _dbRepository.AsQueryable().ToList();
                _logger.LogInformation($"製品のリストが取得されました");
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "製品のリストを取得中にエラーが発生しました");
                return StatusCode(500, $"内部サーバーエラー: {ex.Message}");
            }
        }
        [HttpGet("product/{id}")]
        public async Task<ActionResult<Products>> GetProductById(string id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }
                var product = await _dbRepository.FindById(id);
                if (product == null)
                {
                    return NotFound($"ID {id} не найден");
                }
                _logger.LogInformation($"Продукт с ID {id} был найден");
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при поиске продукта с ID {id}");
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetUniqueCategories()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("User is not authenticated");
                }
                var uniqueCategories =  _dbRepository.AsQueryable().Select(p => p.category).Distinct().ToList();
                _logger.LogInformation($"Unique categories retrieved");
                return Ok(uniqueCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while retrieving unique categories");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpGet("products/category/{category}")]
        public async Task<ActionResult<IEnumerable<Products>>> GetProductsByCategory(string category)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }
                var products = _dbRepository.AsQueryable().Where(p => p.category.ToLower() == category.ToLower()).ToList();
                if (products.Count == 0)
                {
                    return NotFound($"No products found in category: {category}");
                }
                _logger.LogInformation($"Products in category {category} retrieved successfully");
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while retrieving products in category {category}");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpDelete("product/{id}")]
        public async Task<ActionResult> DeleteProductById(string id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }
                var product = await _dbRepository.FindById(id);
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }
                _dbRepository.DeleteById(id);
                _logger.LogInformation($"Product with ID {id} has been deleted successfully");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while deleting product with ID {id}");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpPatch("product/{id}/isdeleted")]
        public async Task<ActionResult> UpdateIsDeleted(string id, [FromBody] string isDeleted)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }
                if (!bool.TryParse(isDeleted, out bool isDeletedValue))
                {
                    return BadRequest("Неверное значение для поля IsDeleted. Значение должно быть true или false.");
                }
                var product = await _dbRepository.FindById(id);
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }
                product.IsDeleted = isDeletedValue;
                _dbRepository.ReplaceOne(product);
                _logger.LogInformation($"IsDeleted value for product with ID {id} has been updated to {isDeletedValue}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while updating IsDeleted value for product with ID {id}");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
