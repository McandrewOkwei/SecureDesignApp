using FinalProject.Data;
using FinalProject.Models;
using FinalProject.Services.Aes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Services
{

    //Class is to be injected into the controller to obsure user data before writing to db and to decrypt user data when reading from the db.
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;

        }

        //Grab user aes key for decryption of their sensitive data
        public async Task<byte[]> GetUserKeyAsync(string username)
        {

            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            if (username != currentUsername)
            {
                throw new UnauthorizedAccessException("You aren't authorized to this user's data. Better luck next time broe.");
            }
            var user = await _context.Users
                .Where(u => u.Username == username)
                .Select(u => u.UserKey)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            return user;
        }
        
    }
}
