using Common;
using NRediSearch;
using Redis_POC.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis_POC
{
    class Program
    {
        private static int mode = 4;
        static async Task Main(string[] args)
        {
            // Mode:
            // 1 for read and write
            // 2 for Search
            // 3 for pub/sub 
            // 4 for test stream

            Console.WriteLine("Welcome to Device Managment System");
            switch (mode)
            {
                case 0:
                    {
                        await PopulateData();
                        break;
                    }
                case 1:
                    {
                        await PrintData();
                        break;
                    }
                case 2:
                    {
                        await ManageSearch();
                        break;
                    }
                case 3:
                    {
                        await ManageCommunication();
                        break;
                    }
                case 4:
                    {
                        await ManageStreamAsync();
                        break;
                    }
                default:
                    break;
            }

            Console.ReadKey();
        }
       
        public static async Task PopulateData()
        {
            await ReadWriteHandler.PopulateData();

        }

        public static async Task PrintData()
        {
            await ReadWriteHandler.Read();
        }

        public static async Task ManageSearch()
        {
            await SearchHandler.CreateIndexes();
            Console.WriteLine("\nPress \n1 for search by Brand \n2 for search by Model  \n3 for Search by Price");

            var choice = Console.ReadLine();
            int result;

            if (!int.TryParse(choice, out result))
            {
                Console.WriteLine("Invalid option, exiting..");
            }

            if (result == 1)
            {
                Console.WriteLine("Enter the Brand name");
                var searchResults = await SearchHandler.SearchByBrand(Console.ReadLine());

                if (searchResults.Count == 0)
                {
                    Console.WriteLine("No matching data found");
                }
                else
                {
                    PrintResults(searchResults);
                }
            }
            else if (result == 2)
            {
                Console.WriteLine("Enter the Model");
                var searchResults = await SearchHandler.SearchByModel(Console.ReadLine());

                if (searchResults.Count == 0)
                {
                    Console.WriteLine("No matching data found");
                }
                else
                {
                    PrintResults(searchResults);
                }
            }
            else if (result == 3)
            {
                Console.WriteLine("Enter the Price");
                var searchResults = await SearchHandler.SearchByPrice(Console.ReadLine());

                if (searchResults.Count == 0)
                {
                    Console.WriteLine("No matching data found");
                }
                else
                {
                    PrintResults(searchResults);
                }
            }
            else
            {
                Console.WriteLine("Invalid option.");
            }
            await ManageConsole();
        }
        private static async Task ManageCommunication()
        {
            Console.WriteLine("\nType mesage to send:");
            await Publisher.SendMessageAsync(Console.ReadLine(), Constants.DeviceManagementChannle);

            Console.WriteLine("\nPress R to send another message or any pther key to exit");
            if (Console.ReadKey().Key == ConsoleKey.R)
            {
                Console.WriteLine();
                await ManageCommunication();
            }
        }
        private static void PrintResults(List<Document> docs)
        {
            Console.WriteLine("\nResults:\n");
            foreach (var doc in docs)
            {
                Console.WriteLine("ID: " + doc.Id + "\n");
                foreach (var prop in doc.GetProperties())
                {
                    Console.WriteLine(prop.Key + ":" + prop.Value);
                }
                Console.WriteLine();
            }
        }

        private static async Task ManageConsole()
        {
            Console.WriteLine("Press R to search or press any other key to exit");

            if (Console.ReadKey().Key == ConsoleKey.R)
            {
                Console.WriteLine();
                await ManageSearch();
            }
            return;
        }

        public static async Task ManageStreamAsync()
        {
            await StreamHandler.ManageStreamAsync();
        }
    }
}