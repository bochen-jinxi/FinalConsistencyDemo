using System.Linq;
using System.Numerics;
using System;
using System.Text;

namespace FinalConsistencyDemo.Consumers
{
    public static class StringSharding
    {
        /// <summary>
        /// 将 str 视作无符号 128 位整数，计算它对 shardCount 的取模值。
        /// </summary>
        /// <param name="str">待分片的 str/param>
        /// <param name="shardCount">分片总数（比如 64）</param>
        /// <returns>0 .. shardCount-1 之间的分片 ID</returns>
        public static int ModShard(this String str, int shardCount)
        {
            if (shardCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(shardCount));

            // 1. 把 GUID 转为 16 字节小端序
            byte[] bytes = Encoding.Default.GetBytes(str);

            // 2. 在高位追加一个 0 字节，确保BigInteger当作无符号处理
            byte[] unsignedBytes = bytes.Concat(new byte[] { 0 }).ToArray();

            // 3. 用小端字节构造 BigInteger
            BigInteger value = new BigInteger(unsignedBytes);

            // 4. 取模（确保结果为非负）
            BigInteger mod = value % shardCount;
            if (mod < 0)
                mod += shardCount;

            return (int)mod;
        }
    }
}
