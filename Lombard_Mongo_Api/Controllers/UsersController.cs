using Lombard_Mongo_Api.Helpers;
using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Lombard_Mongo_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Tracing;
using System.Linq.Expressions;

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
        private readonly IMongoRepository<Users> _UserRepository;
        private readonly IMongoRepository<Lombards> _LombardRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Fuji> _logger;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _contextAccessor;
        public UsersController(IConfiguration configuration, IMongoRepository<Users> dbRepository, ILogger<Fuji> logger, IUserService userRepository,IMongoRepository<Lombards> lombardsrepository, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _UserRepository = dbRepository;
            _logger = logger;
            _userService = userRepository;
            _LombardRepository = lombardsrepository;
            _contextAccessor = httpContextAccessor;
        }
        [HttpGet("GetUser")]
        public async Task<ActionResult> GetUser(string id)
        {
            try 
            {
                var user = await _UserRepository.FindById(id);
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
        [HttpGet("GetUserInfo")]
        public async Task<ActionResult> GetUserInfo(string id)
        {
            try 
            {
                var user = await _UserRepository.FindById(id);
                var userinfo = new UsersGetInfoDto()
                {
                    username = user.username,
                    number = user.number,
                    email = user.email,
                    _idLombard = user._idLombard
                };
                return Ok(userinfo);    
            }
            catch(Exception ex) 
            {
                return BadRequest(ex);
            }
        }
        [HttpPost("AddMod")]
        public async Task<ActionResult> AddMod(ModDto obj)
        {
            try
            {
                // Подготовьте лямбда-выражение для фильтрации
                Expression<Func<Users, bool>> filterExpression = u => u.username == obj.username;
                // Вызовите метод FindOne с этим фильтром
                var user = await _UserRepository.FindOne(filterExpression);
                if (user != null)
                {
                    return BadRequest("This username already exist, please choose another one");
                }

                var emailCheck = await _UserRepository.FindOne(p => p.email == obj.email);
                if (emailCheck !=null)
                {
                    return BadRequest("Account with such email already exis. Choose another one or log into you account");
                }
                var LombardName =await  _LombardRepository.FindOne(p=> p.lombard_name == obj.LombardName);

                if(LombardName == null)
                {
                    return BadRequest("This Lombard does not exist");
                }
                else
                {
                    _userService.CreatePasswordHash(obj.password, out byte[] passwordHash, out byte[] passwordSalt);

                    var users = new Users
                    {
                        Id = "",
                        username = obj.username,
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        role = Enums.Role.Moderator.ToString(),
                        email = obj.email,
                        number = obj.number,
                        _idLombard = LombardName.Id
                    };
                    _UserRepository.InsertOne(users);
                    return Ok();
                }
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("UpdateMod")]
        public async Task<ActionResult> UpdateMod(string id ,ModUpdateDto Dto)
        {
            try
            {

                var user = _contextAccessor.HttpContext.User;
                var userId = user.Claims.FirstOrDefault(c => c.Type == "UserId");
                var transac = _UserRepository.FindById(id);
                if (transac != null)
                {

                    var transaction = new Users
                    {
                        Id = transac.Result.Id,
                        username = Dto.username,
                        PasswordHash = transac.Result.PasswordHash,
                        PasswordSalt = transac.Result.PasswordSalt,
                        role = Enums.Role.Moderator.ToString(),
                        email = Dto.email,
                        number = Dto.number,
                        _idLombard = transac.Result._idLombard
                    };
                    _UserRepository.ReplaceOne(transaction);
                    return Ok();
                }
                else
                {
                    return NotFound("User deos not found");
                }
            
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("UpdateUser")]
        public async Task<ActionResult> UpdateUser(string id, ModUpdateDto Dto)
        {
            try
            {

                var user = _contextAccessor.HttpContext.User;
                var userId = user.Claims.FirstOrDefault(c => c.Type == "UserId");
                var transac = _UserRepository.FindById(id);
                if (transac != null)
                {

                    var transaction = new Users
                    {
                        Id = transac.Result.Id,
                        username = Dto.username,
                        PasswordHash = transac.Result.PasswordHash,
                        PasswordSalt = transac.Result.PasswordSalt,
                        role = Enums.Role.User.ToString(),
                        email = Dto.email,
                        number = Dto.number,
                        _idLombard = null
                    };
                    _UserRepository.ReplaceOne(transaction);
                    return Ok();
                }
                else
                {
                    return NotFound("User deos not found");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteTheUser")]
        public async Task<ActionResult> DeleteTheUser(string id)
        {
            try
            {
                var user = _UserRepository.FindById(id);
                if(user != null)
                {
                    _UserRepository.DeleteById(id);
                    return Ok();
                }
                else
                {
                    return NotFound("Such user does not exist");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("AddAdmin")]
        public async Task<ActionResult> AddAdmin(UsersDto obj)
        {
            try
            {
                // Подготовьте лямбда-выражение для фильтрации
                Expression<Func<Users, bool>> filterExpression = u => u.username == obj.username;
                // Вызовите метод FindOne с этим фильтром
                var user = await _UserRepository.FindOne(filterExpression);
                if (user != null)
                {
                    return BadRequest("This username already exist, please choose another one");
                }
                var emailCheck = await _UserRepository.FindOne(p => p.email == obj.email);
                if (emailCheck != null)
                {
                    return BadRequest("Account with such email already exis. Choose another one or log into you account");
                }
                else
                {
                    _userService.CreatePasswordHash(obj.password, out byte[] passwordHash, out byte[] passwordSalt);

                    var users = new Users
                    {
                        Id = "",
                        username = obj.username,
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        role = Enums.Role.Admin.ToString(),
                        email = obj.email,
                        number = obj.number,
                        _idLombard = null
                    };
                    _UserRepository.InsertOne(users);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
      
    }

}
