using Redis_POC.Connections;
using System;
using Common;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Subscriber
{
    internal class Program
    {
        static void Main(string[] args)
        {
            EnableKeyspaceNotifications();

            //SubscribeToDemoDeviceCommunicationChannel();
            //SubscribeACutomeKeySpaceEventNotifications();
            //SubscribeAllKeySpaceEventNotifications();
            SubscribeUserDataUpdateNotifications();
            Console.ReadLine();
        }
        private static void SubscribeToDemoDeviceCommunicationChannel()
        {
            Console.WriteLine("Listening " + Constants.DeviceManagementChannle);

            var subscriber = RedisConnector.GetSubscriber();
            subscriber.Subscribe(Constants.DeviceManagementChannle, (channel, message)
                 => Console.Write("\nMessage received in: " + Constants.DeviceManagementChannle + " " + message));

        }
        /// <summary>
        /// Listens to User Output Update Stream notifications
        /// </summary>
        private static void SubscribeUserDataUpdateNotifications()
        {
            Console.WriteLine("Listening " + Constants.UserOutputUpdate);

            var subscriber = RedisConnector.GetSubscriber();
            var keyspace = "__keyspace@0__:" + Constants.UserOutputUpdate;
            subscriber.Subscribe(keyspace, (channel, message)
            =>
            {
                Console.Write($"\nUser Data updated \nOperation: {message}\n");
                ReadUserUpdateStream();
            });
        }

        /// <summary>
        /// Enables key space notifciaitons so when value for a key is changes, a notificaiton is generated
        /// </summary>
        private static void EnableKeyspaceNotifications()
        {
            var server = RedisConnector.GetServer();
            //"AKE" string means all the events 
            server.ConfigSet("notify-keyspace-events", "AKE");
        }

        /// <summary>
        /// Enables key space notifciaitons for the given key in input
        /// </summary>
        private static async Task SubscribeACutomeKeySpaceEventNotifications()
        {
            Console.WriteLine("Enter the key to monitor");
            var key = Console.ReadLine();
            var keyspace = "__keyspace@0__:" + key;
            Console.WriteLine($"Listening to keyspace {keyspace} for {key}");
            var subscriber = RedisConnector.GetSubscriber();
            var db = RedisConnector.GetDatabase();
            string previousValueKey = "prvs" + Reverse(key);
            string currentKey = "crnt" + Reverse(key);
            string previousValue = null;
            string currentValue = db.StringGet(currentKey);

            if (db.KeyExists(previousValueKey))
            {
                previousValue = db.StringGet(previousValueKey);
            }

            subscriber.Subscribe(keyspace, (channel, message)
                  =>
            {
                if (message == "del")
                {
                    Console.WriteLine($"\nKey Deleted \nValue at deletion: { currentValue} \n Value before deletion: {previousValue}");
                }
                if (message == "set")
                {
                    Console.WriteLine($"\nKey Updated \nValue before updation: {previousValue} \nValue after updation:{currentValue}");
                }
            });
        }

        /// <summary>
        /// Reads data from the stream 
        /// </summary>
        private static void  ReadUserUpdateStream()
        {
            var db = RedisConnector.GetDatabase();
            string streamName = "User-Update-Output";

            // '-' means lowest id, and '+' means highest id
            var result = db.StreamRange(streamName, "-", "+", 1, Order.Descending);
            if (result.Any())
            {
                var dict = ParseResult(result.First());
                foreach (var item in dict)
                {
                    Console.WriteLine($"{item.Key}: {item.Value}");
                }
            }
        }

        /// <summary>
        /// Parse stream entry to dictionary as key value pair
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private static Dictionary<string, string> ParseResult(StreamEntry entry) =>
           entry.Values.ToDictionary
           (
               x => x.Name.ToString(),
               x => x.Value.ToString()
           );

        /// <summary>
        /// Captures data change for all the keys
        /// </summary>
        /// <returns></returns>
        private static async Task SubscribeAllKeySpaceEventNotifications()
        {
            Console.WriteLine("Listening All Keys");

            var subscriber = RedisConnector.GetSubscriber();
            subscriber.Subscribe("__key*__:*", (channel, message)
                 =>
            {
                Console.Write("\nMessage received in all keys change: " + message);

            });
        }
        /// <summary>
        /// Reverses the gien string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}