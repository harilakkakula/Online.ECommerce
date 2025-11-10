using Microsoft.EntityFrameworkCore;
using UserCreation.Business.Entities;

namespace UserCreation.Business.Context
{
    public class UserDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefOrder> Orders { get; set; }
    }
}
