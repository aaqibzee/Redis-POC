using Redis_POC.Connections;
using System;
using Common;
using System.Threading;

namespace Subscriber
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SubscribeChannel();
            Console.ReadLine();
        }

        private static void SubscribeChannel()
        {
            Console.WriteLine("Listening " + Constants.DeviceManagementChannle);
            var subscriber = RedisConnector.GetSubscriber();
            subscriber.Subscribe(Constants.DeviceManagementChannle, (channel, message)
                 => Console.Write("\nMessage received in: " + Constants.DeviceManagementChannle + " " + message));

        }
    }
}
