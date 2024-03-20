using Lombard_Mongo_Api.Helpers;
using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.Models.Dtos;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Lombard_Mongo_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
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
        private readonly IMongoRepository<Lombards> _LombardsRepository;
        private readonly IHttpContextAccessor _contextAccessor;

        public TransactionHistoryController(IMongoRepository<TransactionHistory> dbRepository, IHttpContextAccessor httpContextAccessor,
            IMongoRepository<Products> products, IMongoRepository<Users> usersrepository, IMongoRepository<Lombards> lombardsRepository)
        {
            _TransactionRepository = dbRepository;
            _contextAccessor = httpContextAccessor;
            _productsRepository = products;
            _usersRepository = usersrepository;
            _LombardsRepository = lombardsRepository;
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
        [HttpGet("GetMyTransactions/{userId}")]
        public async Task<ActionResult<IEnumerable<List<TransactionProductDto>>>> GetMyTransactions(string userId)
        {
            try
            {
                if (!ObjectId.TryParse(userId, out ObjectId userIdObjectId))
                {
                    return BadRequest("Invalid userId format");
                }

                var myTransactions = _TransactionRepository.AsQueryable()
                    .Where(p => p._idUser == userId)
                    .ToList();

                var transactionProductList = new List<List<TransactionProductDto>>();

                foreach (var transaction in myTransactions)
                {
                    var product = await _productsRepository.FindById(transaction._idProduct);

                    if (product != null)
                    {
                        var lombard = await _LombardsRepository.FindById(product._idLombard);

                        transactionProductList.Add(new List<TransactionProductDto>
                {
                    new TransactionProductDto { Transaction = transaction, Product = product, Lombard = lombard }
                });
                    }
                }

                if (transactionProductList.Any())
                {
                    return Ok(transactionProductList);
                }
                else
                {
                    return NotFound("You didn't make any transactions.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                // Получение идентификатора пользователя из токена
                var userId = User.FindFirst("UserId")?.Value;

                var productcheck = await _productsRepository.FindById(obj._idProduct.ToString());

                if (productcheck != null)
                {
                    var transaction = new TransactionHistory
                    {
                        Id = "",
                        _idUser = userId, // Используем идентификатор пользователя из токена
                        _idProduct = obj._idProduct.ToString(),
                        status = Enums.TransactionState.InQue.ToString(),
                    };
                    _TransactionRepository.InsertOne(transaction);

                    // Добавление ссылки на новую транзакцию в список "мои транзакции" пользователя
                    var userEntity = await _usersRepository.FindById(userId);
                    if (userEntity != null)
                    {
                        if (userEntity.MyTransactions == null)
                        {
                            userEntity.MyTransactions = new List<string>();
                        }
                        userEntity.MyTransactions.Add(transaction.Id);
                        _usersRepository.ReplaceOne(userEntity);
                    }

                    var product = new Products
                    {
                        Id = productcheck.Id,
                        name = productcheck.name,
                        category = productcheck.category,
                        Brand = productcheck.Brand,
                        ImageFileName = productcheck.ImageFileName,
                        description = productcheck.description,
                        price = productcheck.price,
                        status = Enums.revengeancestatus.Reserved.ToString(),
                        IsDeleted = productcheck.IsDeleted,
                        _idLombard = productcheck._idLombard
                    };
                    _productsRepository.ReplaceOne(product);
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

        [HttpPut("CancelTransaction")]
        public async Task<ActionResult> CancelTransaction(string id)
        {
            try
            {
                var transaction = await _TransactionRepository.FindById(id);

                if (transaction == null)
                {
                    return NotFound("Such transaction does not exist");
                }

                var product = await _productsRepository.FindById(transaction._idProduct);

                if (product == null)
                {
                    return NotFound("Associated product not found");
                }

                // Update transaction status to Rejected
                transaction.status = Enums.TransactionState.Rejected.ToString();
                _TransactionRepository.ReplaceOne(transaction);

                // Update product status to In_stock
                product.status = Enums.revengeancestatus.In_stock.ToString();
                _productsRepository.ReplaceOne(product);

                return Ok("Transaction canceled successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("CompleteTransaction")]
        public async Task<ActionResult> CompleteTransaction(string id)
        {
            try
            {
                var transaction = await _TransactionRepository.FindById(id);

                if (transaction == null)
                {
                    return NotFound("Such transaction does not exist");
                }

                var product = await _productsRepository.FindById(transaction._idProduct);

                if (product == null)
                {
                    return NotFound("Associated product not found");
                }

                // Update transaction status to Completed
                transaction.status = Enums.TransactionState.Completed.ToString();
                _TransactionRepository.ReplaceOne(transaction);

                // Update product status to Bought
                product.status = Enums.revengeancestatus.Bought.ToString();
                _productsRepository.ReplaceOne(product);

                return Ok("Transaction completed successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
