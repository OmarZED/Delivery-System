using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Models;

namespace WebApplication3.Maping
{

    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
     : base(options) { }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Basket> Baskets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Dish>()
     .Property(d => d.Category)
     .HasConversion<string>();

            // Configure the relationship between User and Basket (one-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Basket)
                .WithOne(b => b.User)
                .HasForeignKey<Basket>(b => b.UserId);

            // Configure the relationship between Basket and BasketItem (one-to-many)
            modelBuilder.Entity<Basket>()
                .HasMany(b => b.BasketItems)
                .WithOne(bi => bi.Basket)
                .HasForeignKey(bi => bi.BasketId);


            // Configure the relationship between BasketItem and Dish (many-to-one)
            modelBuilder.Entity<BasketItem>()
                .HasOne(bi => bi.Dish)
                .WithMany() // Or WithMany(d => d.BasketItems) if Dish should have a navigation back
                .HasForeignKey(bi => bi.DishId);


        }
    }
}
