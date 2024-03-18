using Amazon.Runtime.Internal;
using DnsClient;
using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Lombard_Mongo_Api.Services;
using Lombard_Mongo_Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cors;
namespace Lombard_Mongo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("_myAllowSpecificOrigins")]
    public class AuthorizationController : Controller
    {
        private readonly IMongoRepository<Users> _dbRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        public AuthorizationController(IMongoRepository<Users> dbRepository, IConfiguration configuration, IUserService userRepository)
        {
            _dbRepository = dbRepository;
            _configuration = configuration;
            _userService = userRepository;
        }
        [HttpPost("Login")]
        public async Task<ActionResult> Get(LoginDto login)
        {
            try
            {
                // Подготовьте лямбда-выражение для фильтрации
                Expression<Func<Users, bool>> filterExpression = u => u.username == login.username;
                // Вызовите метод FindOne с этим фильтром
                var user = await _dbRepository.FindOne(filterExpression);
                // Если пользователь не найден, верните сообщение об ошибке
                if (user == null)
                {
                    return NotFound("User not found");
                }
                // Проверяем пароль
                if (!_userService.VerifyPasswordHash(login.password, user.PasswordHash, user.PasswordSalt))
                {
                    return Unauthorized("Invalid password");
                }

                var claims = new List<Claim>
                {
                      new Claim("UserId", user.Id.ToString()),
                      new Claim(ClaimTypes.Role.ToString() , user.role)
                };

                SymmetricSecurityKey GetSymmetricSecurityKey() =>
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

                // Создаем JWT-токен
                var jwt = new JwtSecurityToken(
                    issuer: _configuration["JwtSettings:Issuer"],
                    audience: _configuration["JwtSettings:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromHours(3)),
                    signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                var response = new
                {
                    encodedJwt = encodedJwt,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Registration")]
        public async Task<ActionResult> Post(UsersDto obj)
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
                var emailCheck = await _dbRepository.FindOne(p => p.email == obj.email);
                if (emailCheck != null)
                {
                    return BadRequest("Account with such email already exis. Choose another one or log into you account");
                }
                _userService.CreatePasswordHash(obj.password, out byte[] passwordHash, out byte[] passwordSalt);

                var users = new Users
                {
                    Id = "",
                    username = obj.username,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    role = Enums.Role.User.ToString(),
                    email = obj.email,
                    number = obj.number,
                    _idLombard = null
                };
                _dbRepository.InsertOne(users);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
    }
}