﻿using DnsClient;
using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Microsoft.AspNetCore.Http;
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
namespace Lombard_Mongo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : Controller
    {
        private readonly IMongoRepository<Users> _dbRepository;
        private readonly IConfiguration _configuration;
        public AuthorizationController(IMongoRepository<Users> dbRepository, IConfiguration configuration)
        {
            _dbRepository = dbRepository;
            _configuration = configuration;
        }
        [HttpPost("Login")]
        public ActionResult Get(LoginDto login)
        {
            try
            {
                // Подготовьте лямбда-выражение для фильтрации
                Expression<Func<Users, bool>> filterExpression = u => u.username == login.username && u.password == login.password;
                // Вызовите метод FindOne с этим фильтром
                var user = _dbRepository.FindOne(filterExpression);
                // Если пользователь найден, верните его
                if (user != null)
                {
                    List<Claim> claims =
                        [
                            new Claim(ClaimTypes.UserData, user.Id.ToString()),
                            new Claim(ClaimTypes.Role, user.role)
                        ];
                    SymmetricSecurityKey GetSymmetricSecurityKey() =>
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
                    // создаем JWT-токен
                    var jwt = new JwtSecurityToken(
                            issuer: _configuration["JwtSettings:Issuer"],
                            audience: _configuration["JwtSettings:Audience"],
                            claims: claims,
                            expires: DateTime.UtcNow.Add(TimeSpan.FromHours(24)),
                            signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)); ; ;
                    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                    var response = new
                    {
                        access_token = encodedJwt,
                    };
                    return Ok(response);
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

        [HttpPost("Registration")]
        public IActionResult Post(UsersDto obj)
        {
            try
            {
                // Подготовьте лямбда-выражение для фильтрации
                Expression<Func<Users, bool>> filterExpression = u => u.username == obj.username ;
                // Вызовите метод FindOne с этим фильтром
                var user = _dbRepository.FindOne(filterExpression);
                if(user != null)
                {
                    return BadRequest("This username already exist, please choose another one");
                }
                var users = new Users
                {
                    Id = "",
                    username = obj.username,
                    password = obj.password,
                    role = "User",
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
