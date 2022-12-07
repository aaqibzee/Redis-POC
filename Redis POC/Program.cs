using Common;
using NRediSearch;
using Redis_POC.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Handlers;

namespace Redis_POC
{
    class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to Device Managment System");
            int mode = SelectMenu();

            
            switch (mode)
            {
                case 1:
                    {
                        await PopulateData();
                        break;
                    }
                case 2:
                    {
                        await PrintData();
                        break;
                    }
                case 3:
                    {
                        await ManageSearch();
                        break;
                    }
                case 4:
                    {
                        await ManageCommunication();
                        break;
                    }
                case 5:
                    {
                        await ManageStreamAsync();
                        break;
                    }
                case 6:
                    {
                        await ManageKeySpaceNotification();
                        break;
                    }
                default:
                    break;

            }
            Console.ReadKey();
        }

        private static int SelectMenu()
        {

            Console.WriteLine("\nSelect from the menu \n1 For populating the data \n2 For listing the saved data " +
                "\n3 For Searching the data \n4 For doing communication \n5 For watching price update stream");

            var input = Console.ReadKey().Key;
            if (!(input > ConsoleKey.NumPad0 && input <= ConsoleKey.NumPad5))
            {
                Console.WriteLine("\nInvalid selection, choose the right option");
                SelectMenu();
            }

            return ConvertConsoleKeyToInt(input);
        }

        public static async Task PopulateData()
        {
            Console.WriteLine("\nRunning Populate Data Module Now");
            await ReadWriteHandler.PopulateData();

        }

        public static async Task PrintData()
        {
            Console.WriteLine("\nRunning Read Data Module Now");
            await ReadWriteHandler.Read();
        }

        public static async Task ManageSearch()
        {
            Console.WriteLine("\nRunning Search Module Now");
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
        private static async Task ManageKeySpaceNotification()
        {
            Console.WriteLine("\nEnter the brand name");
            var brand = Console.ReadLine();

            Console.WriteLine("\nEnter the model name");
            var model = Console.ReadLine();

            Console.WriteLine("\nPrice");
            var price = Console.ReadLine();


            Console.WriteLine("\nType the Id to to send:");
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

        private static async Task ManageStreamAsync()
        {
            Console.WriteLine("\nRunning Stream Module Now");
            await StreamHandler.PublishDeviceUpdateStreamAsync();
        }

        private static int ConvertConsoleKeyToInt(ConsoleKey key)
        {
            if(key==ConsoleKey.NumPad1)
                return 1;
           if(key==ConsoleKey.NumPad2)
                return 2;
           if(key==ConsoleKey.NumPad3)
                return 3;
           if(key==ConsoleKey.NumPad4)
                return 4;
           else
                return 5;
        }
    }
}