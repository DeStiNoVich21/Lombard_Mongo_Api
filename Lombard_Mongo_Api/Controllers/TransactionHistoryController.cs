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
        private readonly IMongoRepository<TransactionHistory> _TransactionRepository;
        private readonly IMongoRepository<Products> _productsRepository;
        private readonly IMongoRepository<Users> _usersRepository;
        private readonly IHttpContextAccessor _contextAccessor;

        public TransactionHistoryController(IMongoRepository<TransactionHistory> dbRepository, IHttpContextAccessor httpContextAccessor,
            IMongoRepository<Products> products, IMongoRepository<Users> usersrepository)
        {
            _TransactionRepository = dbRepository;
            _contextAccessor = httpContextAccessor;
            _productsRepository = products;
            _usersRepository = usersrepository;
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
                var products = _TransactionRepository.AsQueryable().ToList();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error {ex.Message}");
            }
        }

        [HttpGet("GetMyTransactions")]
        public async Task<ActionResult<IEnumerable<TransactionHistory>>> GetMyTransactions(string id)
        {
            try
            {
                var mytransactions = _TransactionRepository.AsQueryable().Where(p => p._idUser == id);
                if (mytransactions != null)
                {
                    return Ok(mytransactions);
                }
                else
                {
                    return NotFound("You didnt made any transactions");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("GetTheTransaction")]
        public async Task<ActionResult<IEnumerable<TransactionHistory>>> GetTransaction(string id)
        {
            try
            {
                var transaction = await _TransactionRepository.FindById(id);
                if(transaction != null)
                {
                    return Ok(transaction);
                }
                else
                {
                    return NotFound("Such transaction does not exist");
                }
            }
            catch
            {
                return BadRequest("Can`t get the transaction");
            }
        }

        [HttpGet("GetLombardTransactions")]
        public async Task<ActionResult<IEnumerable<TransactionHistory>>> GetLombardTransactions()
        {
            try
            {
                var userId = _contextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
                var user = _usersRepository.FindById(userId.ToString());
                var transactions = _TransactionRepository.FindById(user.Result._idLombard);
                if(transactions != null)
                {
                    return Ok(transactions);
                }
                else
                {
                    return NotFound("Lombard didnt made any transactions yet");
                }
            }
            catch (Exception ex) 
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("BuyTheProduct")]
        public async Task<ActionResult> BuyTheProduct(TransactionDto obj)
        {
            try
            {

                var user = _contextAccessor.HttpContext.User;
                var userId = user.Claims.FirstOrDefault(c => c.Type == "UserId");

                var productcheck = _productsRepository.FindById(obj._idProduct.ToString()) ;

                if (productcheck != null)
                {
                    var transaction = new TransactionHistory
                    {
                        Id = "",
                        _idUser = userId.ToString(),
                        _idProduct = obj._idProduct.ToString(),
                        status = obj.status
                    };
                    _TransactionRepository.InsertOne(transaction);
                    return Ok();
                }
                else
                {
                    return NotFound("Such product does not exist");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateTransactionStatus")]
        public async Task<ActionResult> UpdateStatus(TransactionUpdateDto dto)
        {
            try
            {

                var user = _contextAccessor.HttpContext.User;
                var userId = user.Claims.FirstOrDefault(c => c.Type == "UserId");
                var transac = _TransactionRepository.FindById(dto.Id);
                if(transac != null) 
                {
                    return NotFound("Transaction does not exist");
                }
                var transaction = new TransactionHistory
                {
                    Id = dto.Id,
                    _idUser = userId.ToString(),
                    _idProduct = transac.Result._idProduct.ToString(),
                    status = dto.status
                };
                _TransactionRepository.ReplaceOne(transaction);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        


    }
}
