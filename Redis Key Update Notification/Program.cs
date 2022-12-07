using Common.Handlers;
using Redis.OM;
using System.Text.Json;
using Test_App;
{
    RedisConnectionProvider provider = new RedisConnectionProvider("redis://localhost:6379");
    List<Customer> allCustomers = new List<Customer>()
    {
        new Customer() { Id="1", FirstName = "One", LastName = "Hundered", Age = 20, Email = "one@email.com", Address = new Address{ City="One City"}},
        new Customer() { Id="2", FirstName = "Two", LastName = "Hundered", Age = 21, Email = "two@email.com", Address = new Address{ City="Two City"}},
        new Customer() { Id="3", FirstName = "Three", LastName = "Hundered", Age = 22, Email = "three@email.com", Address = new Address{ City="Three City"}},
        new Customer() { Id="4", FirstName = "Four", LastName = "Hundered", Age = 23, Email = "four@email.com", Address = new Address{ City="Four City"}},
        new Customer() { Id="5", FirstName = "Five", LastName = "Hundered", Age = 24, Email = "five@email.com", Address = new Address{ City="Five City"}},
        new Customer() { Id="6", FirstName = "Six", LastName = "Hundered", Age = 25, Email = "six@email.com", Address = new Address{ City="Six City"}},
        new Customer() { Id="7", FirstName = "Seven", LastName = "Hundered", Age = 26, Email = "seven@email.com", Address = new Address{ City="Seven City"}},
        new Customer() { Id="8", FirstName = "Eight", LastName = "Hundered", Age = 27, Email = "eight@email.com", Address = new Address{ City="Eight City"}},
        new Customer() { Id="9", FirstName = "Nine", LastName = "Hundered", Age = 28, Email = "nine@email.com", Address = new Address{ City="Nine City"}},
        new Customer() { Id="10", FirstName = "Ten", LastName = "Hundered", Age = 29, Email = "ten@email.com", Address = new Address{ City="Ten City"}},
    };
    { 
        provider.Connection.CreateIndex(typeof(Customer));

        for (int counter = 0; counter < 3; counter = counter + 1)
        {
            InsertEntity<Customer>(allCustomers[counter]);
        }

        Console.WriteLine("All customers inserted succesfully. ");
        Console.ReadLine();
    }

    void InsertEntity<T>(T current) where T : IRedisCollection
    {
        var previos = Get<Customer>(current.Id);
        InsertOrUpdate<T>(current);

        var dataToSend = new
        {
            Previous = previos,
            Current = current,
            Type = typeof(T).Name.ToString()
        };

        var data = JsonSerializer.Serialize(dataToSend);
        var stream = typeof(T).Name + "-Output";

        Publisher.PostToStream(stream, "Customer-" + current.Id, data);
    }
    T Get<T>(string key) where T : IRedisCollection
    {
        var entities = provider.RedisCollection<T>();

        if (entities.Count() > 0)
        {
            return entities.ToList().FirstOrDefault(x => x.Id == key);
        }

        return default(T);
    }

    IEnumerable<T> GetAll<T>()
    {
        return provider.RedisCollection<T>();
    }

    void InsertOrUpdate<T>(T data) where T : IRedisCollection
    {
        var result = provider.Connection.CreateIndex(typeof(T));
        var entities = provider.RedisCollection<T>();

        if (entities.ToList().FirstOrDefault(x => x.Id == data.Id) != null)
        {
            entities.Update(data);
        }

        entities.Insert(data);
    }
}