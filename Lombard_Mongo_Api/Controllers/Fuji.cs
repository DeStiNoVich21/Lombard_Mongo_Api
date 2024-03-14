﻿using Lombard_Mongo_Api.Helpers;
using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
namespace Lombard_Mongo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("_myAllowSpecificOrigins")]
    public class Fuji : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMongoRepository<Products> _dbRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Fuji> _logger;
        public Fuji(IConfiguration configuration, IMongoRepository<Products> dbRepository, ILogger<Fuji> logger, IWebHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _dbRepository = dbRepository;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpPost("addProduct")]
        [Authorize]
        public async Task<ActionResult> AddProduct([FromForm] ProductsDto productDto, IFormFile image)
        {
            try
            {
                if (productDto == null)
                {
                    return BadRequest("Отсутствуют данные о продукте.");
                }
                if (image == null || image.Length == 0)
                {
                    return BadRequest("Требуется файл изображения.");
                }
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("Пользователь не авторизован");
                }
                if (productDto.price < 0)
                {
                    return BadRequest("Цена не может быть отрицательной.");
                }
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                string relativeImagePath = Path.Combine("material", fileName);
                string imagePath = Path.Combine(_hostingEnvironment.ContentRootPath, relativeImagePath);
                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
                productDto.status = Enums.revengeancestatus.In_stock.ToString();
                var product = new Products
                {
                    name = productDto.name,
                    category = productDto.category,
                    ImageFileName = fileName,
                    description = productDto.description,
                    price = productDto.price,
                    status = productDto.status,
                    IsDeleted = productDto.IsDeleted,
                    LombardId = productDto.LombardId,
                    Brand = productDto.brand  // Добавляем поле бренд
                };
                _dbRepository.InsertOne(product);
                _logger.LogInformation($"Продукт был добавлен: {product.name}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при добавлении продукта");
                return StatusCode(500, $"Произошла ошибка: {ex.Message}");
            }
        }
        [HttpGet("products")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Products>>> GetProducts()
        {
            try
            {
                var products = _dbRepository.AsQueryable().Where(p => !p.IsDeleted).ToList();
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
        [AllowAnonymous]
        public async Task<ActionResult<Products>> GetProductById(string id)
        {
            try
            {
                var product = await _dbRepository.FindById(id);
                if (product == null)
                {
                    return NotFound($"ID {id} не найден");
                }
                _logger.LogInformation($"Продукт с ID {id} был найден");
                return Ok(new { product.Id, product.name, product.category, product.description, product.price, product.status, product.IsDeleted, product.Brand, product.ImageFileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при поиске продукта с ID {id}");
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }
        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetCategoriesWithMatchingImageNamesAndProductNames()
        {
            try
            {
                var allProducts = await _dbRepository.GetAllAsync(); // Загрузка всех продуктов из базы данных
                var matchingProducts = allProducts
                    .Select(p => new { Category = p.category.ToLower(), ImageName = Path.GetFileNameWithoutExtension(p.ImageFileName)?.ToLower(), Name = p.name }) // Добавляем поле имени
                    .Where(p => p.ImageName != null && p.Category == p.ImageName)
                    .Select(p => new { Category = p.Category, ImageFileName = $"{p.ImageName}.png", Name = p.Name }) // Добавляем поле имени
                    .ToList();
                _logger.LogInformation($"Список продуктов с категорией и именем изображения, совпадающими, получен успешно");
                return Ok(matchingProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении списка продуктов с одинаковой категорией и именем изображения");
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }
        [HttpGet("products/category/{category}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Products>>> GetProductsByCategory(string category)
        {
            try
            {
                var products = _dbRepository.AsQueryable().Where(p => p.category.ToLower() == category.ToLower() && !p.IsDeleted).ToList();
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
        [HttpPatch("product/{id}/isdeleted")]
        [Authorize]
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
        [HttpGet("deletedProducts")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Products>>> GetDeletedProducts()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("User is not authenticated");
                }
                var deletedProducts = _dbRepository.AsQueryable().Where(p => p.IsDeleted).ToList();
                _logger.LogInformation($"Deleted products retrieved successfully");
                return Ok(deletedProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving deleted products");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpGet("getImage/{imageName}")]
        [AllowAnonymous]
        public IActionResult GetImage(string imageName)
        {
            try
            {
                var imagePath = Path.Combine(_hostingEnvironment.ContentRootPath, "material", imageName);
                if (!System.IO.File.Exists(imagePath))
                {
                    return NotFound();
                }
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return File(imageBytes, "image/jpeg"); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Произошла ошибка: {ex.Message}");
            }
        }
        [HttpPatch("product/{id}/status")]
        [Authorize]
        public async Task<ActionResult> UpdateProductStatus(string id, [FromBody] string status)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("User is not authenticated");
                }
                if (!Enum.IsDefined(typeof(Enums.revengeancestatus), status))
                {
                    return BadRequest("Invalid status value. Status must be either 'Reserved' or 'In_stock'.");
                }
                var product = await _dbRepository.FindById(id);
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }
                product.status = status;
                _dbRepository.ReplaceOne(product);
                _logger.LogInformation($"Status for product with ID {id} has been updated to {status}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while updating status for product with ID {id}");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpGet("products/filter")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Products>>> GetFilteredProducts(string brand = null, int? minPrice = null, int? maxPrice = null, string category = null)
        {
            try
            {
                if (minPrice.HasValue && minPrice < 0)
                {
                    return BadRequest("Минимальная цена не может быть отрицательной.");
                }

                if (maxPrice.HasValue && maxPrice < 0)
                {
                    return BadRequest("Максимальная цена не может быть отрицательной.");
                }
                var categoryProductsResponse = await GetProductsByCategory(category);
                if (!(categoryProductsResponse.Result is OkObjectResult) || !(categoryProductsResponse.Result is ActionResult<IEnumerable<Products>>))
                {
                    return categoryProductsResponse;
                }
                var categoryProducts = ((OkObjectResult)categoryProductsResponse.Result).Value as IEnumerable<Products>;
                var filteredProducts = categoryProducts.Where(p => !p.IsDeleted);
                if (!string.IsNullOrEmpty(brand))
                {
                    filteredProducts = filteredProducts.Where(p => p.Brand.ToLower() == brand.ToLower());
                }
                if (minPrice.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => p.price >= minPrice.Value);
                }
                if (maxPrice.HasValue && maxPrice != 0)
                {
                    filteredProducts = filteredProducts.Where(p => p.price <= maxPrice.Value);
                }
                return Ok(filteredProducts.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при фильтрации продуктов");
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }
        [HttpPost("addProductWithCategoryAndImage")]
        [Authorize]
        public async Task<ActionResult> AddProductWithCategoryAndImage([FromForm] string name, [FromForm] string category, IFormFile image)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest("Отсутствует имя продукта.");
                }
                if (string.IsNullOrEmpty(category))
                {
                    return BadRequest("Отсутствует категория продукта.");
                }
                if (image == null || image.Length == 0)
                {
                    return BadRequest("Требуется файл изображения.");
                }
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("Пользователь не авторизован");
                }
                string fileName = $"{category.ToLower().Replace(" ", "")}.png";
                string relativeImagePath = Path.Combine("material", fileName);
                string imagePath = Path.Combine(_hostingEnvironment.ContentRootPath, relativeImagePath);
                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
                var product = new Products
                {
                    ImageFileName = fileName,
                    category = category,
                    name = name,
                    IsDeleted = true // Устанавливаем значение IsDeleted в true
                };
                _dbRepository.InsertOne(product);
                _logger.LogInformation($"Продукт '{name}' с изображением был добавлен в категорию: {category}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при добавлении продукта с изображением");
                return StatusCode(500, $"Произошла ошибка: {ex.Message}");
            }
        }
    }
}
