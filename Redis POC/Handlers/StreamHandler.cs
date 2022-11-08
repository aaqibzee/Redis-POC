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
        public static async Task ManageStreamAsync()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var db= RedisConnector.GetDatabase();

            const string streamName = "telemetry";
            const string groupName = "avg";

           //Check if stream exist, if so, we can check if group exists. Incase, any of one doesn't we can create the group then
            if (!(await db.KeyExistsAsync(streamName))
                || (await db.StreamGroupInfoAsync(streamName)).All(x => x.Name != groupName))
            {
                await db.StreamCreateConsumerGroupAsync(streamName, groupName, "0-0", true);
            }

            //Stream producer
            //Write a random number between 50 and 65 as the temp and send the current time as the time
            //Repeat after every 2 seconds, till the token is cancelled
            var producerTask = Task.Run(async () =>
            {
                var random = new Random();
                while (!token.IsCancellationRequested)
                {
                    await db.StreamAddAsync(streamName,
                        new NameValueEntry[]
                            {
                                new("temp", random.Next(50, 65)),
                                new NameValueEntry("time", DateTimeOffset.Now.ToUnixTimeSeconds())
                            });
                    await Task.Delay(2000);
                }
            });

            //Parse stream results
            Dictionary<string, string> ParseResult(StreamEntry entry) => 
                entry.Values.ToDictionary
                (
                    x => x.Name.ToString(),
                    x => x.Value.ToString()
                );

            //Read from stream
            var readTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    // '-' means lowest id, and '+' means highest id
                    var result = await db.StreamRangeAsync(streamName, "-", "+", 1, Order.Descending);
                    if (result.Any())
                    {
                        var dict = ParseResult(result.First());
                        Console.WriteLine($"Read result: temp {dict["temp"]} time: {dict["time"]}");
                    }

                    await Task.Delay(1000);
                }
            });

            double count = default;
            double total = default;
            //Send acknowledgment to server, and read from the stream group 
            ////
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
                    var result = await db.StreamReadGroupAsync(streamName, groupName, "avg-1", ">", 1);
                    //Read from the group and print results
                    if (result.Any())
                    {
                        id = result.First().Id;
                        count++;
                        var dict = ParseResult(result.First());
                        total += double.Parse(dict["temp"]);
                        Console.WriteLine($"Group read result: temp: {dict["temp"]}, time: {dict["time"]}, current average: {total / count:00.00}");
                    }
                    await Task.Delay(1000);
                }
            });

            //Cancel the token after 20 second, to avoid running it forever
            tokenSource.CancelAfter(TimeSpan.FromSeconds(20));
            await Task.WhenAll(producerTask, readTask, consumerGroupReadTask);
        }

        public static async Task PublishDeviceCheckoutStreamAsync()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var db = RedisConnector.GetDatabase();

            const string streamName = "telemetry";
            const string groupName = "avg";

            //Check if stream exist, if so, we can check if group exists. Incase, any of one doesn't we can create the group then
            if (!(await db.KeyExistsAsync(streamName))
                || (await db.StreamGroupInfoAsync(streamName)).All(x => x.Name != groupName))
            {
                await db.StreamCreateConsumerGroupAsync(streamName, groupName, "0-0", true);
            }

            //Stream producer
            //Write a random number between 50 and 65 as the temp and send the current time as the time
            //Repeat after every 2 seconds, till the token is cancelled
            var producerTask = Task.Run(async () =>
            {
                var random = new Random();
                while (!token.IsCancellationRequested)
                {
                    await db.StreamAddAsync(streamName,
                        new NameValueEntry[]
                            {
                                new("temp", random.Next(50, 65)),
                                new NameValueEntry("time", DateTimeOffset.Now.ToUnixTimeSeconds())
                            });
                    await Task.Delay(2000);
                }
            });

            //Parse stream results
            Dictionary<string, string> ParseResult(StreamEntry entry) =>
                entry.Values.ToDictionary
                (
                    x => x.Name.ToString(),
                    x => x.Value.ToString()
                );

            //Read from stream
            var readTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    // '-' means lowest id, and '+' means highest id
                    var result = await db.StreamRangeAsync(streamName, "-", "+", 1, Order.Descending);
                    if (result.Any())
                    {
                        var dict = ParseResult(result.First());
                        Console.WriteLine($"Read result: temp {dict["temp"]} time: {dict["time"]}");
                    }

                    await Task.Delay(1000);
                }
            });

            double count = default;
            double total = default;
            //Send acknowledgment to server, and read from the stream group 
            ////
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
                    var result = await db.StreamReadGroupAsync(streamName, groupName, "avg-1", ">", 1);
                    //Read from the group and print results
                    if (result.Any())
                    {
                        id = result.First().Id;
                        count++;
                        var dict = ParseResult(result.First());
                        total += double.Parse(dict["temp"]);
                        Console.WriteLine($"Group read result: temp: {dict["temp"]}, time: {dict["time"]}, current average: {total / count:00.00}");
                    }
                    await Task.Delay(1000);
                }
            });

            //Cancel the token after 20 second, to avoid running it forever
            tokenSource.CancelAfter(TimeSpan.FromSeconds(20));
            await Task.WhenAll(producerTask, readTask, consumerGroupReadTask);
        }
    }
}
