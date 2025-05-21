using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace FinalConsistencyDemo.Data
{
    public class ShardDbContextFactory
    {
        private readonly IDictionary<int, string> _connMap;
        public ShardDbContextFactory(IDictionary<int, string> connMap)
            => _connMap = connMap;

        public ShardDbContext Create(int shardId)
        {
            if (shardId < 0 || shardId >= _connMap.Count)
                throw new ArgumentOutOfRangeException(nameof(shardId));

            var options = new DbContextOptionsBuilder<ShardDbContext>()
                .UseSqlServer(_connMap[shardId])
                .Options;

            return new ShardDbContext(options);
        }
    }
}