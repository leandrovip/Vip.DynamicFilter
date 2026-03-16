using System.ComponentModel;

namespace Vip.DynamicFilter.Tests.Models;

public enum ClientEnum
{
    [Description("Padrão")] Padrao = 0,
    [Description("Especial")] Especial = 1,
    [Description("Serviço")] Servico = 2
}