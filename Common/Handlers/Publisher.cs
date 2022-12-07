using Common.Connections;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Common.Handlers
{
    public class Publisher
    {
        public static async Task SendMessageAsync(string message, string  channel)
        {
            var subscriber = RedisConnector.GetSubscriber();
            var result=await subscriber.PublishAsync(channel, message, CommandFlags.FireAndForget);
            Console.Write($"Message Successfully sent to {channel}. \nClients who recived this message: {result}");
        }
        public static void PostToStream(string stream, string key, string data)
        {
            var db = RedisConnector.Connection.GetDatabase();
            db.StreamAdd(stream, new NameValueEntry[]
            {
                new NameValueEntry(key, data)
            });
        }
    }
}