using Redis_POC.Connections;
using System;

namespace Redis_POC.ReadWrite
{
    internal class RedisReadWriteHandler
    {
        public static void ReadData()
        {
            var cache = RedisConnector.GetCache();
            var devicesCount = 50;
            for (int i = 0; i < devicesCount; i++)
            {
                var value = cache.StringGet($"Device:{i}");
                Console.WriteLine($"Value= {value}");
            }
        }

        public static void SaveBigData()
        {
            var devicesCount = 50;
            var rnd = new Random();
            var cache = RedisConnector.GetCache();

            for (int i = 1; i < devicesCount; i++)
            {
                var value = rnd.Next(0, 50);
                cache.StringSet($"Device:{i}", value);
            }
        }
    }
}
