using Common;
using Redis_POC.Connections;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Redis_POC.Handlers
{
    public static class StreamHandler
    {
        public static async Task PublishDeviceUpdateStreamAsync()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var db = RedisConnector.GetDatabase();

            const string streamName = "device-price-update-telemetry";
            const string groupName = "device-price-update-group";

            //Create strem and consumer group
            if (!(await db.KeyExistsAsync(streamName))
                || (await db.StreamGroupInfoAsync(streamName)).All(x => x.Name != groupName))
            {
                await db.StreamCreateConsumerGroupAsync(streamName, groupName, "0-0", true);
            }

            Task producerTask = PublishDeviceUpdateStream(db, streamName, token);
            Task streamReadTask = ReadDeviceUpdateStream(db, streamName, token);
            //Task consumerGroupReadTaskA = ReadDeviceUpdateStreamAsGroupConsumer(db, streamName, groupName, "group-a-consumer-1", token);
            //Task consumerGroupReadTaskB = ReadDeviceUpdateStreamAsGroupConsumer(db, streamName, groupName, "group-a-consumer-2", token);

            //Run for only x seconds
            tokenSource.CancelAfter(TimeSpan.FromSeconds(3));
            //await Task.WhenAll(producerTask, streamReadTask);
            //await Task.WhenAll(producerTask, consumerGroupReadTaskA, consumerGroupReadTaskB);
            //await Task.WhenAll(producerTask, streamReadTask, consumerGroupReadTaskA, consumerGroupReadTaskB);
        }

        private static Task PublishDeviceUpdateStream(IDatabase db, string streamName, CancellationToken token)
        {
            var producerTask = Task.Run(async () =>
            {
                var random = new Random();
                while (!token.IsCancellationRequested)
                {
                    var deviceId = random.Next(1, Constants.DevicesCount);
                    var price = random.Next(Constants.PriceLowerRange, Constants.PriceUpperRange);
                    await UpdateDevicePrice(price,deviceId);

                    await db.StreamAddAsync(streamName,
                        new NameValueEntry[]
                            {
                                new NameValueEntry("device-id",deviceId)
                            });

                    await Task.Delay(500);
                }
            });
            return producerTask;
        }

        private static Task ReadDeviceUpdateStream(IDatabase db, string streamName, CancellationToken token)
        {
            string deviceId;
            var readTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    // '-' means lowest id, and '+' means highest id
                    var result = await db.StreamRangeAsync(streamName, "-", "+", 1, Order.Descending);
                    if (result.Any())
                    {
                        var dict = ParseResult(result.First());
                        deviceId = dict["device-id"];
                        Console.WriteLine($"\nDevice:{deviceId}'s information updated");
                        Console.WriteLine($"Updated info:");
                        var value = await db.HashGetAllAsync($"Device:{deviceId}");
                        Console.WriteLine($"\nBrand: {value[0].Value} \nModel: {value[1].Value} \nPrice: {value[2].Value}");
                    }

                    await Task.Delay(1000);
                }
            });
            return readTask;
        }
        private static Task ReadDeviceUpdateStreamAsGroupConsumer(IDatabase db, string streamName, string groupName, string consumerName, CancellationToken token)
        {
            string deviceId;
            var consumerGroupReadTask = Task.Run(async () =>
            {

                string id = string.Empty;
                while (!token.IsCancellationRequested)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        // Send an acknowledgment to the server that the id was processed
                        await db.StreamAcknowledgeAsync(streamName, groupName, id);
                        id = string.Empty;
                    }
                    //Read from consumer group
                    var result = await db.StreamReadGroupAsync(streamName, groupName, consumerName, ">", 1);
                    //Read from the group and print results
                    if (result.Any())
                    {
                        id = result.First().Id;
                        var dict = ParseResult(result.First());
                        Console.WriteLine($"From {groupName} {consumerName}");
                        deviceId = dict["device-id"];
                        Console.WriteLine($"\nDevice:{deviceId}'s information updated");
                        Console.WriteLine($"Updated info:");
                        var value = await db.HashGetAllAsync($"Device:{deviceId}");
                        Console.WriteLine($"\nBrand: {value[0].Value} \nModel: {value[1].Value} \nPrice: {value[2].Value}");
                    }
                    await Task.Delay(1000);
                }
            });
            return consumerGroupReadTask;
        }

        private static Dictionary<string, string> ParseResult(StreamEntry entry) =>
        entry.Values.ToDictionary
        (
            x => x.Name.ToString(),
            x => x.Value.ToString()
        );

        private static async Task UpdateDevicePrice(int price, int deviceId)
        {
            var db = RedisConnector.GetDatabase();
            var random = new Random();
            var value = await db.HashGetAllAsync($"Device:{deviceId}");

            await db.HashSetAsync($"Device:{deviceId}", new HashEntry[]
            {
                        new HashEntry("Brand", value[0].Value),
                        new HashEntry("Model", value[1].Value),
                        new HashEntry("Price", price)
            });
        }
    }
}
