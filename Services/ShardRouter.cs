using FinalConsistencyDemo.Consumers;
using FinalConsistencyDemo.Data;
using System.Threading.Tasks;

namespace FinalConsistencyDemo.Services
{
    public class ShardRouter
    {
        private readonly CentralDbContext _central;
        public ShardRouter(CentralDbContext central)
        {
            _central = central;
        }

        // 这里按账号计算简易路由，生产中可查 user_shard_map 表
        public Task<int> GetShardIdAsync(string account)
        {
            var last = account.ModShard(2);
            return Task.FromResult(last);  // 0 或 1
        }
    }
}