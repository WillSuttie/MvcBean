using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MvcBean.Models;

namespace MvcBean.Data
{
    public class MvcBeanContext(DbContextOptions<MvcBeanContext> options) : IdentityDbContext<IdentityUser, IdentityRole, string>(options)
    {
        public DbSet<Bean> Beans { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set the default schema for non-Identity tables
            modelBuilder.HasDefaultSchema("dbo");

            // Map Identity tables to the "Identity" schema
            modelBuilder.Entity<IdentityUser>().ToTable("AspNetUsers", "Identity");
            modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles", "Identity");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles", "Identity");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens", "Identity");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", "Identity");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims", "Identity"); // Re-added

            // Configure the Beans table in the "dbo" schema
            modelBuilder.Entity<Bean>().ToTable("Beans");

            // Configure precision for PricePer100g
            modelBuilder.Entity<Bean>()
                .Property(b => b.PricePer100g)
                .HasPrecision(7, 2);

            // Seed data for Beans table
            modelBuilder.Entity<Bean>().HasData(
                new Bean
                {
                    Id = 1,
                    Name = "Arabica",
                    ColourHex = "#8B4513",
                    Aroma = "Fruity",
                    PricePer100g = 5.50M,
                    SaleDate = DateOnly.Parse("2025-01-01"),
                    ImagePath = "placeholder.jpg"
                },
                new Bean
                {
                    Id = 2,
                    Name = "Robusta",
                    ColourHex = "#654321",
                    Aroma = "Earthy",
                    PricePer100g = 4.00M,
                    SaleDate = DateOnly.Parse("2025-01-02"),
                    ImagePath = "placeholder.jpg"
                }
            );
        }
    }
}
