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
        public async Task<ActionResult<string>> Get(LoginDto login)
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
                // Генерируем рефреш токен
                var refreshclaim = new List<Claim>
        {
            new Claim("Username", user.username.ToString())
        };
                var refreshJwt = GenerateRefreshToken(refreshclaim);

                return Ok(refreshJwt);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]);
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    ValidateLifetime = false // Не проверять срок действия токена
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;

                // Проверяем, что токен обновления имеет правильный формат и содержит все необходимые данные
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return BadRequest("Invalid refresh token");
                }

                // Получаем идентификатор пользователя из токена обновления
                // Получаем имя пользователя из refresh token
                var username = principal.FindFirst("Username").Value;
                Expression<Func<Users, bool>> filterExpression = u => u.username == username;
                var user = _dbRepository.FindOne(filterExpression).Result;

                var claims = new List<Claim>
                {
                      new Claim("UserId", user.Id.ToString()),
                      new Claim(ClaimTypes.Role.ToString() , user.role)
                };
                // Генерируем новый access token
                var accessToken =  GenerateAccessToken(claims);

                var response = new
                {
                    accessToken = accessToken
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

        private string GenerateAccessToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.Add(TimeSpan.FromHours(1)); // Устанавливаем срок действия refresh token

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.Add(TimeSpan.FromDays(30)); // Устанавливаем срок действия refresh token

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}