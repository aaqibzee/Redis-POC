using Redis_POC.Connections;
using System;
using Common;
using System.Threading.Tasks;

namespace Subscriber
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //SubscribeCommunicationChannel();
            EnableKeyspaceNotifications();
            SubscribeKeySpaceEventNotifications();
            //SubscribeAllKeySpaceEventNotifications();
            Console.ReadLine();
        }
        private static void SubscribeCommunicationChannel()
        {
            Console.WriteLine("Listening " + Constants.DeviceManagementChannle);

            var subscriber = RedisConnector.GetSubscriber();
            subscriber.Subscribe(Constants.DeviceManagementChannle, (channel, message)
                 => Console.Write("\nMessage received in: " + Constants.DeviceManagementChannle + " " + message));

        }

        private static void EnableKeyspaceNotifications()
        {
            var server = RedisConnector.GetServer();
            //"AKE" string means all the events 
            server.ConfigSet("notify-keyspace-events", "AKE");
        }

        private static async Task SubscribeKeySpaceEventNotifications()
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
                if (message=="del")
                {
                    Console.WriteLine($"\nKey Deleted \nValue at deletion: { currentValue} \n Value before deletion: {previousValue}");
                }

                if (message == "set")
                {
                    Console.WriteLine($"\nKey Updated \nValue before updation: {previousValue} \nValue after updation:{currentValue}");
                }
            });
        }
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
        private static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
