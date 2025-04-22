using Microsoft.EntityFrameworkCore;
using FinalProject.Models;
namespace FinalProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            :   base(options)
        {
        }

        // DbSet property to handle user model here
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Username).HasColumnName("user");
                entity.Property(e => e.Password).HasColumnName("password");
                entity.Property(e => e.UserKey).HasColumnName("individual_aes_key");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.EncBalance).HasColumnName("EncBalance");
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("cart_id");
                entity.Property(e => e.Username).HasColumnName("user");

                // Define relationship to User
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.Username);
            });

            modelBuilder.Entity<CartItem>(entity => {
                entity.Property(e => e.Id).HasColumnName("cart_item_id");
                entity.Property(e => e.CartId).HasColumnName("cart_id");
                entity.Property(e => e.EncryptedData).HasColumnName("encrypted_data");
                entity.Property(e => e.IV).HasColumnName("iv");


                // Define relationship to Cart
                entity.HasOne(ci => ci.Cart)
                      .WithMany(c => c.Items)
                      .HasForeignKey(ci => ci.CartId);
            });
        }


    }
}