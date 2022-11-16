using NRediSearch;
using Redis_POC.Connections;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis_POC.Handlers
{
    internal class SearchHandler
    {
        private static string indexName = "device-index";
        private static IDatabase db = RedisConnector.GetDatabase();
        private static Client searchClient = new Client(indexName, db);

        public static async Task<List<Document>> SearchByBrand(string text)
        {
            var query = new Query($"{text}");
            query.Limit(0, 1000);

            var results = (await searchClient.SearchAsync(query)).Documents;

            if (results == null)
            {
                return null;
            }
            return results;
        }

        public static async Task<List<Document>> SearchByModel(string text)
        {
            var query = new Query($"{text}");
            query.Limit(0, 1000);
            var results = (await searchClient.SearchAsync(query)).Documents;

            if (results == null)
            {
                return null;
            }
            return results;
        }

        public static async Task<List<Document>> SearchByPrice(string text)
        {
            var query = new Query($"@Price:{text}");
            query.Limit(0, 1000);
            var results = (await searchClient.SearchAsync(query)).Documents;

            if (results == null)
            {
                return null;
            }
            return results;
        }

        public static async Task CreateDeviceIndex()
        {
            searchClient = new Client(indexName, db);
            try
            {
                db.Execute("FT.DROPINDEX", indexName);
            }
            catch (Exception) { }

            var schema = new Schema();
            //Create index on these fields
            schema.AddTextField("Brand");
            schema.AddTextField("Model");
            schema.AddTextField("Price");
            //Index alll the keys, that start with this pattern
            var options = new Client.ConfiguredIndexOptions(new Client.IndexDefinition
            (
                prefixes: new[] { "Device:" })
            );

            await searchClient.CreateIndexAsync(schema, options);
        }

        public static async Task CreateIndexes()
        {
            await CreateDeviceIndex();
        }
    }
}