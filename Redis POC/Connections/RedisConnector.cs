using StackExchange.Redis;
using System;

namespace Redis_POC.Connections
{
    public class RedisConnector
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection;
        static RedisConnector()
        {
            lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect("localhost");
            });
        }

        private static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }


        public static IDatabase GetCache()
        {
                return Connection.GetDatabase();
        }
    }
}
