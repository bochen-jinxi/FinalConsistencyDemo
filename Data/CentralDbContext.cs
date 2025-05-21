using DotNetCore.CAP;
using FinalConsistencyDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalConsistencyDemo.Data
{
    public class CentralDbContext : DbContext
    {
        public CentralDbContext(DbContextOptions<CentralDbContext> opts) : base(opts) { }

        public DbSet<TranLog> TranLogs { get; set; }
        public DbSet<MessageQueue> MessageQueues { get; set; }

        // CAP 自带表：cap.published、cap.received
        // 这些通过 UseEntityFramework<CentralDbContext>() 自动创建
    }
}