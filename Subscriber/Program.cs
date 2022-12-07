using System;
using Common;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Common.Connections;
using ServiceStack;
using Newtonsoft.Json;

namespace Subscriber
{
     public class StreamValue
    {
        public Customer Current { get; set; }
        public Customer Previous { get; set; }
        public string Type { get; set; }
    }
    public struct GeoLoc
    {
        public double Longitude {get; set;}
        public double Latitude {get; set;}
    }

    public partial class Address
    {
        public string StreetName { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public Address ForwardingAddress { get; set; }
        public GeoLoc Location { get; set; }
        public int HouseNumber { get; set; }
    }
    public class Customer
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string[] NickNames { get; set; }
        public Address Address { get; set; }
        public Address AddressAlternate { get; set; }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            EnableKeyspaceNotifications();

            //SubscribeToDemoDeviceCommunicationChannel();
            //SubscribeACutomeKeySpaceEventNotifications();
            //SubscribeAllKeySpaceEventNotifications();
            //SubscribeUserDataUpdateNotifications();
            SubscribeTypeUpdateNotifications<Customer>(new Customer());

            Console.ReadLine();
        }
        /// <summary>
        /// Listens to User Output Update Stream notifications
        /// </summary>
        private static void SubscribeTypeUpdateNotifications<T>(T type)
        {
            string streamName =typeof(T).Name+ "-Output";
            Console.WriteLine("Listening " + streamName);

            var subscriber = RedisConnector.GetSubscriber();
            var keyspace = "__keyspace@0__:" + streamName;
            subscriber.Subscribe(keyspace, (channel, message)
            =>
            {
                ReadTypeUpdateStream<T>(type, streamName);
            });
        }

        /// <summary>
        /// Reads data from the stream 
        /// </summary>
        private static void ReadTypeUpdateStream<T>(T type, string streamNaem)
        {
            var db = RedisConnector.GetDatabase();

            // '-' means lowest id, and '+' means highest id
            var result = db.StreamRange(streamNaem, "-", "+", 1, Order.Descending);

            if (result.Any())
            {
                var dict = ParseResult(result.First());
                DeserializeAndPrintData(dict.ElementAt(0).Key,dict.ElementAt(0).Value);
            }
        }

        private static void DeserializeAndPrintData(string key, string jsonValue)
        {
            object obj="";
            lock(obj)
            {
                var res = JsonConvert.DeserializeObject<StreamValue>(jsonValue);
                Console.WriteLine($"\nKey:{key} Type: {res.Type}");
                if (res.Previous != null)
                Console.WriteLine($"\nPrevious:\nID:{res.Previous.Id}  \nFirst Name: {res.Previous.FirstName} \nLastName: {res.Previous.LastName} \nEmail: {res.Previous.Email} \nCity: {res.Previous.Address.City}");
                Console.WriteLine($"\nCurrent:\nID:{res.Current.Id} \nFirst Name: {res.Current.FirstName} \nLastName: {res.Current.LastName} \nEmail: {res.Current.Email} \nCity: {res.Current.Address.City}");
            }
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
        private static async Task SubscribeToCustomerKeySpaceEventNotifications()
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