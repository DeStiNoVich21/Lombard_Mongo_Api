﻿using Lombard_Mongo_Api.Helpers;
using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Lombard_Mongo_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IMongoRepository<Users> _dbRepository;
        private readonly IMongoRepository<Lombards> _LombardRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Fuji> _logger;
        private readonly IUserService _userService;
        public UsersController(IConfiguration configuration, IMongoRepository<Users> dbRepository, ILogger<Fuji> logger, IUserService userRepository,IMongoRepository<Lombards> lombardsrepository)
        {
            _configuration = configuration;
            _dbRepository = dbRepository;
            _logger = logger;
            _userService = userRepository;
            _LombardRepository = lombardsrepository;
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
        [HttpPost]
        public async Task<ActionResult> AddMod(ModDto obj)
        {
            try
            {
                // Подготовьте лямбда-выражение для фильтрации
                Expression<Func<Users, bool>> filterExpression = u => u.username == obj.username;
                // Вызовите метод FindOne с этим фильтром
                var user = await _dbRepository.FindOne(filterExpression);
                if (user != null)
                {
                    return BadRequest("This username already exist, please choose another one");
                }
                var idlombard =await  _LombardRepository.FindById(obj._idLombard);
                if(idlombard == null)
                {
                    return BadRequest("This Lombard does not exist");
                }
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
                        _idLombard = obj._idLombard
                    };
                    _dbRepository.InsertOne(users);
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