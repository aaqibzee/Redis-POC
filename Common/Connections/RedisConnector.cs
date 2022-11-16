using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Redis_POC.Connections
{
    public class RedisConnector
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection;
       

        static RedisConnector()
        {
            lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                var options = ConfigurationOptions.Parse("localhost:6379");
                options.ConnectRetry = 5;
                options.AllowAdmin = true;
                return ConnectionMultiplexer.Connect(options);
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
        public static IServer GetServer()
        {
            return Connection.GetServer("localhost:6379");
        }

        public static IServer[] GetServers()
        {
            return Connection.GetServers();
        }

        public static ISubscriber GetSubscriber()
        {
            return Connection.GetSubscriber();
        }
    }
}
