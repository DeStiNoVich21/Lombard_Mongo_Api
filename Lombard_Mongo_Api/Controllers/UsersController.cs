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
    //[Authorize(Roles = "admin")] 
    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMongoRepository<Users> _dbRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Fuji> _logger;
        public UsersController(IConfiguration configuration, IMongoRepository<Users> dbRepository, ILogger<Fuji> logger)
        {
            _configuration = configuration;
            _dbRepository = dbRepository;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult> GetUser(string id)
        {
            try 
            {
                var user = await _dbRepository.FindById(id);
                if (user != null)
                {
                    return Ok(user);
                }
                else
                {
                    return NotFound("User not found");
                }
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
