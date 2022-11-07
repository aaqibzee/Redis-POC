using Redis_POC.Connections;
using Redis_POC.ReadWrite;
using System;

namespace Redis_POC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Saving data in cache");
            RedisReadWriteHandler.SaveBigData();

            Console.WriteLine("Reading data from cache");
            RedisReadWriteHandler.ReadData();
            Console.WriteLine("Press anykey to Exit");
            
            Console.ReadKey();
        }

        public static string SearchData(string text)
        {
            var cache = RedisConnector.GetCache();

            var devicesCount = 50;
            var rnd = new Random();
//            var cache = RedisConnector.GetCache();

            for (int i = 1; i < devicesCount; i++)
            {
                var value = rnd.Next(0, 50);
                cache.StringSet($"Device:{i}", value);
            }
        }
    }
}
