using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class User
    {
        [Key]
        [Required] 
        public string Username { get; set; }

        [Required] 
        public string Password { get; set; }

        [Required] 
        public byte[] UserKey { get; set; }

        public string? Email { get; set; } // Nullable email property. They don't need one but is encouraged.

        //Give a user a key every time one is created.
        public User() 
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.KeySize = 256; //All keys 256 bit
            aes.Mode = System.Security.Cryptography.CipherMode.CBC; //Always use Cipher block chaining mode.
            aes.GenerateKey();
            UserKey = aes.Key;
        }
    }
}
