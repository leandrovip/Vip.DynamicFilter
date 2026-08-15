using Vip.DynamicFilter.DemoApi.Models;

namespace Vip.DynamicFilter.DemoApi.Data;

public static class Seed
{
    #region Properties

    public static readonly List<Client> Clients = new()
    {
        Create("Mércia Franco", 5, ClientEnum.Especial, "Rua das Palmeiras"),
        Create("Felícia Santos", 10, ClientEnum.Padrao, "Av 20"),
        Create("Samuel Martins", 14, ClientEnum.Servico, "Rua São Gabriel"),
        Create("Warley Reis", 15, ClientEnum.Especial, "Rua Brasil"),
        Create("Elisa Silva", 22, ClientEnum.Especial, "Rua da Independencia"),
        Create("Jose da Silva", 25, ClientEnum.Padrao, "Rua Marechal Deodoro"),
        Create("Júlio Macedo", 35, ClientEnum.Padrao, "Alameda Brasil"),
        Create("Matheus Braga", 36, ClientEnum.Servico, "Rua Sem Saida"),
        Create("Larissa Silva", 40, ClientEnum.Servico, "Rua Silveiras"),
        Create("Aline Braga", 47, ClientEnum.Padrao, "Rua Tiradentes"),
        Create("Vitor Moreira", 56, ClientEnum.Servico, "Rua 7 de setembro"),
        Create("Gabriel Costa", 60, ClientEnum.Padrao, "Rua Antonio Gaspar"),
        Create("João Xavier", 65, ClientEnum.Especial, "Rua 20"),
        Create("Warley Reis", 78, ClientEnum.Especial, "Rua 21"),
        Create("Sem Endereço", 30, ClientEnum.Padrao, null)
    };

    public static readonly List<Address> Addresses = Clients.SelectMany(x => x.Address).ToList();

    #endregion

    #region Methods

    private static Client Create(string name, int age, ClientEnum clientType, string street)
    {
        var client = new Client
        {
            ClientId = Guid.NewGuid(),
            Name = name,
            Age = age,
            Birthday = DateTime.Now.AddYears(age * -1),
            ClientType = clientType,
            Address = new List<Address>()
        };

        if (!string.IsNullOrWhiteSpace(street))
        {
            client.Address.Add(CreateAddress(client.ClientId, street + " 1", age));
            client.Address.Add(CreateAddress(client.ClientId, street + " 2", age));
            client.Address.Add(CreateAddress(client.ClientId, street + " 3", age));
        }

        return client;
    }

    private static Address CreateAddress(Guid clientId, string street, int zipCode)
    {
        return new Address
        {
            AddressId = Guid.NewGuid(),
            ClientId = clientId,
            Street = street,
            City = "São Paulo",
            ZipCode = zipCode
        };
    }

    #endregion
}
