using Redis.OM.Modeling;
using System;

namespace Test_App
{
    [Document(StorageType = StorageType.Hash, Prefixes = new [] { "Customer" })]
    public class Customer: IRedisCollection
    {
        [RedisIdField]
        [Indexed]
        public string Id { get; set; }
        [Indexed]
        public string FirstName { get; set; }
        [Indexed]
        public string LastName { get; set; }
        public string Email { get; set; }
        [Indexed(Sortable = true)]
        public int Age { get; set; }
        [Indexed]
        public string[] NickNames { get; set; }
        
        [Searchable(JsonPath = "$.StreetName")]
        public Address Address { get; set; }

        [Indexed(CascadeDepth = 2)]
        public Address AddressAlternate { get; set; }
    }

    public interface IRedisCollection
    {
        string Id { get; set; }
    }
}
