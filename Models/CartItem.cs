using FinalProject.Services;
using FinalProject.Services.AES;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using FinalProject.Services.Util;

namespace FinalProject.Models
{
    public class CartItem
    {
        //Cart Item gets its own table with a reference to the user that owns it.

        [Key]
        public int Id { get; set; }

        [Required]
        public int CartId { get; set; }

        [ForeignKey("CartId")] //Relate this item to a given cart that a user in the main db owns.
        public Cart Cart { get; set; }

        [Required]
        public string EncryptedData { get; set; } // Cart item info all smashed into one json string and encrypted.

        [Required]
        public byte[] IV { get; set; } // IV needed for decryption.

        [NotMapped] //Plaintext properties don't need any connection to db, so theyre unmapped.
        public string ProductName { get; set; }

        [NotMapped]
        public int Quantity { get; set; } = 1;

        [NotMapped]
        public decimal Price { get; set; }

       
        
    }

}
