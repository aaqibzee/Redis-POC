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

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        public static IDatabase GetDatabase()
        {
                return Connection.GetDatabase();
        }

        public static ISubscriber GetSubscriber()
        {
            return Connection.GetSubscriber();
        }
    }
}
