using Microsoft.EntityFrameworkCore;
using OrderCreation.Business.Entities;

namespace OrderCreation.Context
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<RefUser> RefUser { get; set; }
    }
}
