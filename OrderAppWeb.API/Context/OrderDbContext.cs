using Microsoft.EntityFrameworkCore;
using OrderAppWeb.API.Models.Entities;

namespace OrderAppWeb.API.Context
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderDetail>()
               .HasOne(d => d.Product)
               .WithMany(p => p.OrderDetails)
               .HasForeignKey(p => p.ProductId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(p => p.Order)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(p => p.OrderId);
        }

    }
}
