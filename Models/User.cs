using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


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

        [NotMapped]
        public double Balance { get; set; } 

        public string EncBalance { get; set; }


        public string? Email { get; set; } // Nullable email property. They don't need one but is encouraged.
       

        //Give a user a key every time one is created.
        public User() 
        {
            //Default Ill give you 50 grans just because im generous. Thank me why dont you?
            Balance = 50000;
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.KeySize = 256; //All keys 256 bit
            aes.Mode = System.Security.Cryptography.CipherMode.CBC; //Always use Cipher block chaining mode.
            aes.GenerateKey();
            UserKey = aes.Key;
            
        }
    }
}
