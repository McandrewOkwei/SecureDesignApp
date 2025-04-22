using FinalProject.Data;
using FinalProject.Migrations;
using FinalProject.Models;
using FinalProject.Services.AES;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FinalProject.Services.Util
{

    //Class is to be injected into the controller to obsure user data before writing to db and to decrypt user data when reading from the db.
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Encryption _encryption;
        public UserService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, Encryption encryption)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _encryption = encryption;
        }

        //Grab user aes key for decryption of their sensitive data
        public async Task<double> GetBalanceAsync(string username)
        {
            var user = await GetUserAsync(username);
            return user.Balance;
        }

        public async Task<User> GetUserAsync(string uname)
        {
            var user = await _context.Users.FindAsync(uname);
            if (user == null)
            {
               throw new Exception("User not found.");
            }
            //if balance is smth other than nothing, decrypt.
            if (!string.IsNullOrEmpty(user.EncBalance))
            {
                try
                {
                    var encryptedBal = Convert.FromBase64String(user.EncBalance);
                    if (encryptedBal.Length < 16)
                    { //Aes works in 16 byte blocks with a 16 byte IV, if less than 16 bytes, something is wrong.
                        throw new Exception("Encrypted balance is empty.");
                    }
                    var iv = new byte[16]; //Make way for iv used to encrypt this.
                    var encBalance = new byte[encryptedBal.Length - 16];
                    Buffer.BlockCopy(encryptedBal, 0, iv, 0, 16); //Grab the iv from the first 16 bytes of the encrypted balance.
                    Buffer.BlockCopy(encryptedBal, 16, encBalance, 0, encryptedBal.Length - 16); //Other bytes are the balance.
                    var decryptedBytes = _encryption.DecryptData(encBalance, user.UserKey, iv);
                    var balStr = Encoding.UTF8.GetString(decryptedBytes);
                    if (double.TryParse(balStr, out double balance))
                    {
                        user.Balance = balance;
                    }
                    else
                    {
                        throw new Exception("Failed to parse decrypted balance.");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to decrypt balance.", ex);
                }
            }
            else
            {
                user.Balance = 55000;
            }
            return user;
        }
        public async Task UpdateBalanceAsync(string username, double newBalance)
        {
            // Ensure only the current user can update their balance
            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (username != currentUsername)
            {
                throw new UnauthorizedAccessException("You aren't authorized to modify this user's balance.");
            }

            var user = await _context.Users.FindAsync(username);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Update the in-memory balance
            user.Balance = newBalance;

            try
            {
                // Generate IV for encryption
                using var aes = System.Security.Cryptography.Aes.Create();
                aes.GenerateIV();
                var iv = aes.IV;

                // Convert balance to string and encrypt it
                var balanceStr = newBalance.ToString();
                var balanceBytes = Encoding.UTF8.GetBytes(balanceStr);
                var encryptedBalance = _encryption.EncryptData(balanceBytes, user.UserKey, iv);

                // smash iv together with balance.
                // Format: [IV (16 bytes)][Encrypted Data]
                var combinedData = new byte[iv.Length + encryptedBalance.Length];
                Buffer.BlockCopy(iv, 0, combinedData, 0, iv.Length);
                Buffer.BlockCopy(encryptedBalance, 0, combinedData, iv.Length, encryptedBalance.Length);

                // Convert to Base64 for storage
                user.EncBalance = Convert.ToBase64String(combinedData);

                // Save changes to the database
                await _context.SaveChangesAsync();
                Console.WriteLine($"Balance updated for user {username}: {newBalance}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update balance: {ex.Message}", ex);
            }
        }
        // Add this new method to your UserService class
        public async Task InitializeUserBalanceAsync(User usr)
        {
            try
            {
                // Generate IV for encryption
                using var aes = System.Security.Cryptography.Aes.Create();
                aes.GenerateIV();
                var iv = aes.IV;

                // Convert balance to string and encrypt it
                var balanceStr = usr.Balance.ToString();
                var balanceBytes = Encoding.UTF8.GetBytes(balanceStr);
                var encryptedBalance = _encryption.EncryptData(balanceBytes, usr.UserKey, iv);

                // smash iv together with balance.
                // Format: [IV (16 bytes)][Encrypted Data]
                var combinedData = new byte[iv.Length + encryptedBalance.Length];
                Buffer.BlockCopy(iv, 0, combinedData, 0, iv.Length);
                Buffer.BlockCopy(encryptedBalance, 0, combinedData, iv.Length, encryptedBalance.Length);

                // Convert to Base64 for storage
                usr.EncBalance = Convert.ToBase64String(combinedData);

                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize user balance.", ex);
            }
        }

        public async Task<bool> ProcessTransactionAsync(string username, double amount)
        {
            try
            {
                // Get user with decrypted balance
                var user = await GetUserAsync(username);

                // Check if user has sufficient funds
                if (user.Balance < amount)
                {
                    return false; // Insufficient funds
                }

                // Update balance
                await UpdateBalanceAsync(username, user.Balance - amount);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing transaction: {ex.Message}");
                return false;
            }
        }
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
        public async Task<string> GetUserPasswordAsync(string username)
        {
            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

           //Assert security context.username matches current user.username.
            if (username != currentUsername)
            {
                throw new UnauthorizedAccessException("You aren't authorized to access this user's data.");
            }

            // Grab bcrypt hashed passwd from db based on this username
            var hashedPassword = await _context.Users
                .Where(u => u.Username == username)
                .Select(u => u.Password)
                .FirstOrDefaultAsync();

            // If the user is not found, throw an exception
            if (hashedPassword == null)
            {
                throw new Exception("User not found.");
            }

            return hashedPassword;
        }

    }
}
