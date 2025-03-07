using Microsoft.EntityFrameworkCore;

namespace Order.API.Models.Context
{
    public class OrderAPIDbContext:DbContext
    {
        public OrderAPIDbContext(DbContextOptions<OrderAPIDbContext> options) : base(options) { }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
