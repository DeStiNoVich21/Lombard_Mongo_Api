using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Lombard_Mongo_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Lombard_Mongo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "admin")] 
    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize]
    public class TransactionHistoryController : Controller
    {
        private readonly IMongoRepository<TransactionHistory> _dbRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        public TransactionHistoryController(IMongoRepository<TransactionHistory> dbRepository,IHttpContextAccessor httpContextAccessor)
        {
            _dbRepository = dbRepository;
            _contextAccessor = httpContextAccessor;

        }

        [HttpGet("GetTransactionsList")]
        public async Task<ActionResult<IEnumerable<TransactionHistory>>> GetTransactionHistory()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized("User is not authenticated");
                }
                var products = _dbRepository.AsQueryable().ToList();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error {ex.Message}");
            }
        }

        [HttpPost("Buy")]
        public async Task<ActionResult> Post(TransactionDto obj)
        {
            try
            {

                var user = _contextAccessor.HttpContext.User;
                var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                var transaction = new TransactionHistory
                {
                    Id = "",
                    _idUser = userId.ToString(),
                    _idProduct = obj._idProduct,
                    status = obj.status
                };
                _dbRepository.InsertOne(transaction);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateStatus(TransactionUpdateDto dto)
        {
            try
            {

                var user = _contextAccessor.HttpContext.User;
                var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                var transac = _dbRepository.FindById(dto.Id);
                var transaction = new TransactionHistory
                {
                    Id = "",
                    _idUser = userId.ToString(),
                    _idProduct = transac.Id.ToString(),
                    status = dto.status
                };
                _dbRepository.ReplaceOne(transaction);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        


    }
}
