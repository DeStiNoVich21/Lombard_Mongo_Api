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
    [Authorize]
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
                // Генерация уникального имени файла
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                // Определение относительного пути
                string relativeImagePath = Path.Combine("material", fileName);
                // Определение абсолютного пути
                string imagePath = Path.Combine(_hostingEnvironment.ContentRootPath, relativeImagePath);
                // Сохранение изображения на файловой системе
                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
                var product = new Products
                {
                    name = productDto.name,
                    category = productDto.category,
                    ImageFileName = fileName, // Сохранение имени файла в базе данных
                    description = productDto.description,
                    price = productDto.price,
                    status = productDto.status,
                    IsDeleted = productDto.IsDeleted
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
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }
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
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Products>>> GetProductsByCategory(string category)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("ユーザーが認証されていません");
                }
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
        [HttpGet("deletedProducts")]
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
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Products>>> SearchProductsByKeywords([FromQuery] string keywords)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keywords))
                {
                    return BadRequest("Keywords cannot be empty");
                }
                // Разбиваем ключевые слова по запятым
                var keywordsList = keywords.Split(',').Select(k => k.Trim()).ToList();
                // Формируем фильтр для поиска по всем полям
                var filter = Builders<Products>.Filter.Or(
                    keywordsList.Select(keyword =>
                        Builders<Products>.Filter.Where(p =>
                            p.name.Contains(keyword) || // Ищем в имени
                            p.category.Contains(keyword) || // Ищем в категории
                            p.description.Contains(keyword) || // Ищем в описании
                            p.price.ToString().Contains(keyword) || // Ищем в цене
                            p.status.Contains(keyword) // Ищем в статусе
                        )
                    )
                );
                var products = await _dbRepository.FindAsync(filter);
                if (products.Count == 0)
                {
                    _logger.LogInformation($"No products found with keywords: {string.Join(", ", keywordsList)}");
                    return NotFound();
                }
                _logger.LogInformation($"Products containing keywords '{string.Join(", ", keywordsList)}' retrieved successfully");
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while searching products with keywords '{keywords}'");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpGet("getImage/{imageName}")]
        [AllowAnonymous]
        public IActionResult GetImage(string imageName)
        {
            try
            {
                // Путь к папке, где хранятся изображения
                var imagePath = Path.Combine(_hostingEnvironment.ContentRootPath, "material", imageName);
                if (!System.IO.File.Exists(imagePath))
                {
                    // Если файл не существует, возвращаем NotFound
                    return NotFound();
                }
                // Чтение содержимого файла
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                // Возвращаем изображение вместе с соответствующим заголовком HTTP
                return File(imageBytes, "image/jpeg"); // Предполагается, что изображение в формате JPEG
            }
            catch (Exception ex)
            {
                // В случае ошибки возвращаем код состояния 500 (внутренняя ошибка сервера)
                return StatusCode(500, $"Произошла ошибка: {ex.Message}");
            }
        }
    }
}
