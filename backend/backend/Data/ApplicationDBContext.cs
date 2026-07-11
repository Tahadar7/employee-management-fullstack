using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                // Unique email — no two users can register with the same address
                entity.HasIndex(u => u.Email).IsUnique();

                // Store the enum as a readable string
                entity.Property(u => u.Role)
                      .HasConversion<string>()
                      .HasMaxLength(20);
            });

            // ---- Employee ----
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasIndex(e => e.Phone)
          .IsUnique()
          .HasFilter("[Phone] IS NOT NULL");    // allow multiple nulls, but unique if not null
            });

            // ---- RefreshToken ----
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(rt => rt.Token);

                // One User has many RefreshTokens.
                // Delete a user -> their refresh tokens are deleted too (cascade).
                entity.HasOne(rt => rt.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}