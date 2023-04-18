using Vip.DynamicFilter.Tests.Models;

namespace Vip.DynamicFilter.Tests.Core;

public class Mock
{
    public static IQueryable<Client> Clients = new List<Client>
    {
        new("Mércia Franco", 5, "Rua das Palmeiras"),
        new("Felícia Santos", 10, "Av 20"),
        new("Samuel Martins", 14, "Rua São Gabriel"),
        new("Warley Reis", 15, "Rua Brasil"),
        new("Elisa Silva", 22, "Rua da Independencia"),
        new("Jose da Silva", 25, "Rua Marechal Deodoro"),
        new("Júlio Macedo", 35, "Alameda Brasil"),
        new("Matheus Braga", 36, "Rua Sem Saida"),
        new("Larissa Silva", 40, "Rua Silveiras"),
        new("Aline Braga", 47, "Rua Tiradentes"),
        new("Vitor Moreira", 56, "Rua 7 de setembro"),
        new("Gabriel Costa", 60, "Rua Antonio Gaspar"),
        new("João Xavier", 65, "Rua 20"),
        new("Warley Reis", 78, "Rua 21")
    }.AsQueryable();
}