using FinalConsistencyDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalConsistencyDemo.Data
{
    public class ShardDbContext : DbContext
    {
        public ShardDbContext(DbContextOptions<ShardDbContext> opts) : base(opts) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<MsgLog> MsgLogs { get; set; }
    }
}