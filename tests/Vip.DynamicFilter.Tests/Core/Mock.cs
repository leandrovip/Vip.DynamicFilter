using Vip.DynamicFilter.Tests.Models;

namespace Vip.DynamicFilter.Tests.Core;

public class Mock
{
    public static IQueryable<Client> Clients = new List<Client>
    {
        new("Mércia Franco", 5, "Rua das Palmeiras", ClientEnum.Especial),
        new("Felícia Santos", 10, "Av 20", ClientEnum.Padrao),
        new("Samuel Martins", 14, "Rua São Gabriel", ClientEnum.Servico),
        new("Warley Reis", 15, "Rua Brasil", ClientEnum.Especial),
        new("Elisa Silva", 22, "Rua da Independencia", ClientEnum.Especial),
        new("Jose da Silva", 25, "Rua Marechal Deodoro", ClientEnum.Padrao),
        new("Júlio Macedo", 35, "Alameda Brasil", ClientEnum.Padrao),
        new("Matheus Braga", 36, "Rua Sem Saida", ClientEnum.Servico),
        new("Larissa Silva", 40, "Rua Silveiras", ClientEnum.Servico),
        new("Aline Braga", 47, "Rua Tiradentes", ClientEnum.Padrao),
        new("Vitor Moreira", 56, "Rua 7 de setembro", ClientEnum.Servico),
        new("Gabriel Costa", 60, "Rua Antonio Gaspar", ClientEnum.Padrao),
        new("João Xavier", 65, "Rua 20", ClientEnum.Especial),
        new("Warley Reis", 78, "Rua 21", ClientEnum.Especial)
    }.AsQueryable();
}