using Bogus;

namespace Vip.DynamicFilter.Tests.Models;

public class Client
{
    #region Properties

    public Guid ClientId { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime Birthday { get; set; }
    public ICollection<Address> Address { get; set; }

    #endregion

    #region Constructor

    public Client(string name, int age, string street)
    {
        var faker = new Faker("pt_BR");

        ClientId = Guid.NewGuid();
        Name = name;
        Age = age;
        Birthday = DateTime.Now.AddDays(age * -1);
        Address = new List<Address>
        {
            new(ClientId, street + " 1", faker.Address.City(), faker.Random.Int(1, 35)),
            new(ClientId, street + " 2", faker.Address.City(), faker.Random.Int(1, 35)),
            new(ClientId, street + " 3", faker.Address.City(), faker.Random.Int(1, 35))
        };
    }

    #endregion
}