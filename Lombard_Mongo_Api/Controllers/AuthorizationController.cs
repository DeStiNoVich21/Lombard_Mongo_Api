using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Lombard_Mongo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthorizationController : Controller
    {
        private readonly IMongoRepository<Users> _dbRepository;

        public AuthorizationController(IMongoRepository<Users> dbRepository)
        {
            _dbRepository = dbRepository;
        }

        [HttpGet]
        public ActionResult Get()
        {
            try
            {
                var query = _dbRepository.AsQueryable();
                return Ok(query);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Post(Users obj)
        {
            try
            {
              
                _dbRepository.InsertOne(obj);

                return Created();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
