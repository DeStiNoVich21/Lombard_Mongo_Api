using Lombard_Mongo_Api.Models;

namespace Lombard_Mongo_Api.Services
{
    public interface IUserService
    {
        Task<string> CreateToken(Users user);
        Task<string> RefreshToken(string token);
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
    }
}
