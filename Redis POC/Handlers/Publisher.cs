using NRediSearch;
using Redis_POC.Connections;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Redis_POC.Handlers
{
    internal class Publisher
    {
        public static async Task SendMessageAsync(string message, string  channel)
        {
            var subscriber = RedisConnector.GetSubscriber();
            await subscriber.PublishAsync(channel, message, CommandFlags.FireAndForget);
            Console.Write("Message Successfully sent to "+ channel);
        }
    }
}