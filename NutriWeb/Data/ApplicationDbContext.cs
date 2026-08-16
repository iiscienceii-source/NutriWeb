using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NutriWeb.Models;

namespace NutriWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;
        public DbSet<PriceGroup> PriceGroups { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserSubscription>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Начальное заполнение тарифных групп с явным типом decimal
            builder.Entity<PriceGroup>().HasData(
                new PriceGroup { Id = 1, Name = "Чистое питание", Description = "Базовые блюда, супы, горячее и соусы", PriceUah = 450m },
                new PriceGroup { Id = 2, Name = "Живое тесто & Сладкое", Description = "Безглютеновый хлеб, выпечка, десерты", PriceUah = 650m },
                new PriceGroup { Id = 3, Name = "Суперфуды & Смузи", Description = "Детокс-напитки, витаминные боулы и эликсиры", PriceUah = 350m }
            );
        }
    }
}