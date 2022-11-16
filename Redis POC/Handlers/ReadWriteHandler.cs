using Common;
using Redis_POC.Connections;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis_POC.Handlers
{
    public static class ReadWriteHandler
    {
        public static  async Task Read()
        {
            Console.WriteLine("Reading data..");
            var db = RedisConnector.GetDatabase();
            for (int i = 1; i <= Constants.DevicesCount; i++)
            {
                var value = await db.HashGetAllAsync($"Device:{i}");
                Console.WriteLine($"Key: Device:{i} Value: { value[0]} {value[1]} {value[2]}"); 
            }
            Console.WriteLine("Done");
        }

        public static async Task PopulateData()
        {
            Console.WriteLine("Populating data..");
            List<string> brands = new List<string>()
            {
                "Apple",
                "Razer",
                "Microsoft",
                "MSI",
                "Asus",
                "Acer",
                "Dell",
                "Lenovo",
                "HP",
                "Toshiba"
            };
            
            var random = new Random();
            var db = RedisConnector.GetDatabase();

            for (int i = 1; i <= Constants.DevicesCount; i++)
            {
                var brand = brands[random.Next(0, brands.Count-1)];
                var model = brand[0].ToString()+ random.Next(100, 500);
                var price = random.Next(Constants.PriceLowerRange, Constants.PriceUpperRange);

                await db.HashSetAsync($"Device:{i}", new HashEntry[] 
                { 
                    new HashEntry("Brand", brand),
                    new HashEntry("Model", model),
                    new HashEntry("Price", price)
                });
            }
             Console.WriteLine("Data Populated");
        }
    }
}
