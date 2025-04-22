using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class Cart
    {

       //Cart gets its own table with a reference to the user that owns it.
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [ForeignKey("Username")] //Relate this cart to a user in main database;
        public User User { get; set; }

        

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
