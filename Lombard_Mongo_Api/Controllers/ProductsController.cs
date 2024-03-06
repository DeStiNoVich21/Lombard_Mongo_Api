using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lombard_Mongo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IMongoRepository<Products> _dbRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductsController> _logger;
        public ProductsController(IConfiguration configuration, IMongoRepository<Products> dbRepository, ILogger<ProductsController> logger)
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
                _logger.LogError(ex, "An error occurred while adding the product");
                return StatusCode(500, $"An error has occurred.: {ex.Message}");
            }
        }

    }
}
