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
        public DbSet<BasketItem> BasketItems { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Rating> Ratings { get; set; }

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

            // Configure the one-to-one relationship between User and Basket entities.
            // Each User has one Basket, and each Basket is linked to exactly one User.
            // The foreign key is Basket.UserId, which enforces this association in the database.
            // This ensures referential integrity and navigational properties for Entity Framework Core.
            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId);

            // Configure the relationship between Order and OrderItem (one-to-many)
            modelBuilder.Entity<Order>()
               .HasMany(o => o.OrderItems)
               .WithOne(oi => oi.Order)
               .HasForeignKey(oi => oi.OrderId);



            // Configure the relationship between OrderItem and Dish (many-to-one)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Dish)
                .WithMany()
                .HasForeignKey(oi => oi.DishId);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Dish)
                .WithMany()
                .HasForeignKey(r => r.DishId);


        }
    }
}
