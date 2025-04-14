using FinalProject.Data;
using Microsoft.EntityFrameworkCore;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace FinalProject.Services.Aes
{
    public class Encryption
    {
        private readonly ApplicationDbContext _context;

        public Encryption(ApplicationDbContext context)
        {
            _context = context;
        }
        //Fetch user Aes Key
        public byte[] GenerateAesKey()
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.KeySize = 256; // AES 256-bit key
            aes.GenerateKey();
            return aes.Key;
        }

        public byte[] EncryptData(byte[] data, byte[] key, byte[] iv)
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;

            using var encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public byte[] DecryptData(byte[] encryptedData, byte[] key, byte[] iv)
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        }
        public static async Task SaveToDbAsync(ApplicationDbContext dbContext, string username, byte[] encryptedKey)
        {
            // Find the user by username
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

            if(user == null)
            {
                throw new Exception("User not found");
            }

            // Update the user's EncryptedKey field
            user.UserKey = encryptedKey;

            // Save changes to the database
            await dbContext.SaveChangesAsync();
        }
    }
}
