using Microsoft.EntityFrameworkCore;
using FinalProject.Models;
namespace FinalProject.Data
{
    public class ApplicationDbContext : DbContext
    {
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
            });
        }
        public DbSet<User> Users { get; set; }
    }
}