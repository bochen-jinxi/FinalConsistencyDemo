using System.Linq;
using DotNetCore.CAP;
using FinalConsistencyDemo.Data;
using FinalConsistencyDemo.Models;
using FinalConsistencyDemo.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalConsistencyDemo.Consumers
{
    public class FinanceConsumer : ICapSubscribe
    {
        private readonly ShardRouter _router;
        private readonly ShardDbContextFactory _factory;
        private readonly CentralDbContext _centralDb;

        public FinanceConsumer(ShardRouter router, ShardDbContextFactory factory , CentralDbContext centralDb)
        {
            _router = router;
            _factory = factory;
            _centralDb = centralDb;
        }
        
        [CapSubscribe("finance.debit")]
        public async Task HandleDebit(dynamic msg)
        {
            // 将 dynamic 转为 JSON 字符串
            string jsonString = JsonSerializer.Serialize(msg);

            // 反序列化为强类型对象
            DebitMessage debit = JsonSerializer.Deserialize<DebitMessage>(jsonString);


            string tranId = debit.TranId;
            string acct = debit.Account;
            decimal delta = debit.Delta;

            if (!await _centralDb.MessageQueues
                    .Where(x => x.TranId == tranId)
                    .Where(x => x.Account == acct)
                    .Where(m => m.Type == "DEBIT")
                    .AnyAsync())
                return;

            ;

            int shardId = await _router.GetShardIdAsync(acct);
            await using var db = _factory.Create(shardId);

            // 幂等检查
            if (await db.MsgLogs.AnyAsync(x => x.LogKey == $"{tranId}-{acct}"))
                return;

            await using var tx = await db.Database.BeginTransactionAsync();
            var account = await db.Accounts.FindAsync(acct);
            account.Balance += delta;
            db.MsgLogs.Add(new MsgLog
            {
                LogKey = $"{tranId}-{acct}",
                TranId = tranId,
                Account = acct
            });
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }

       [CapSubscribe("finance.credit")]
        public async Task HandleCredit(dynamic msg)
        {
            // 将 dynamic 转为 JSON 字符串
            string jsonString = JsonSerializer.Serialize(msg);

            // 反序列化为强类型对象
            DebitMessage credit = JsonSerializer.Deserialize<DebitMessage>(jsonString);

            string tranId = credit.TranId;
            string acct = credit.Account;
            decimal delta = credit.Delta;

            if (!await _centralDb.MessageQueues
                    .Where(x => x.TranId == tranId)
                    .Where(x => x.Account == acct)
                    .Where(m => m.Type == "CREDIT")
                    .AnyAsync())
                return;

            int shardId = await _router.GetShardIdAsync(acct);
            await using var db = _factory.Create(shardId);

            if (await db.MsgLogs.AnyAsync(x => x.LogKey == $"{tranId}-{acct}"))
                return;

            await using var tx = await db.Database.BeginTransactionAsync();
            var account = await db.Accounts.FindAsync(acct);
            account.Balance += delta;
            db.MsgLogs.Add(new MsgLog
            {
                LogKey = $"{tranId}-{acct}",
                TranId = tranId,
                Account = acct
            });
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }
}
