# Uso com EF Core

Como a biblioteca opera sobre `IQueryable<T>`, ela funciona com **EF Core** (e qualquer provedor LINQ) sem configuração adicional.

## Consulta básica (deferred)

```csharp
using Vip.DynamicFilter;

var request = new FilterRequest
{
    Where = new Filter
    {
        OperatorType = Operator.And,
        Filters = new List<Filter>
        {
            new("Age", WhereCondition.GreaterThanOrEqual, 18),
            new("Address.Street", WhereCondition.Contains, "Rua")
        }
    },
    OrderBy = new List<Order>
    {
        new("Name", OrderDirection.Asc)
    },
    PageNumber = 1,
    Limit = 20
};

// IQueryable — a query só é executada no ToList/Count
IQueryable<Client> query = _context.Clients.ApplyFilterRequest(request);
var result = await query.ToListAsync();
```

## Com resposta paginada

```csharp
var response = await _context.Clients.GetFilterResponse(request);
// response.TotalCount já conta os registros filtrados antes da paginação
```

## Em um Controller

```csharp
[ApiController]
[Route("clients")]
public class ClientsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientsController(AppDbContext context) => _context = context;

    [HttpPost("query")]
    public async Task<FilterResponse<Client>> Query([FromBody] FilterRequest request)
    {
        return await _context.Clients.GetFilterResponse(request);
    }
}
```

## Exemplo com EF Core InMemory (teste)

```csharp
using Microsoft.EntityFrameworkCore;

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase("clients")
    .Options;

await using var context = new AppDbContext(options);
context.Clients.AddRange(
    new Client("Mércia Franco", 5, "Rua das Palmeiras", ClientEnum.Especial),
    new Client("Jose da Silva", 25, "Rua Marechal Deodoro", ClientEnum.Padrao));
await context.SaveChangesAsync();

var request = new FilterRequest
{
    Where = new Filter("Name", WhereCondition.Contains, "Silva")
};

var result = context.Clients.ApplyFilterRequest(request).ToList();
// 1 registro: "Jose da Silva"
```

## Observações

- O padrão funciona melhor em cenários com **tradução de expressão** para SQL. Condições que não são traduzíveis pelo provedor (ex.: `Contains` com `StringComparison`) podem exigir a avaliação em memória (`.AsEnumerable()`).
- Para tradução a SQL, prefira as condições `=`, `!=`, `<`, `>`, `<=`, `>=`, `~`, `*~`, `~*`, `any`, `isnull`, `notnull`.
- O exemplo de aplicação real está na pasta [`demo/`](../demo/Vip.DynamicFilter.DemoApi) (Minimal API com `GetFilterResponse`).

## Referência

- [`QueryableExtensions`](../src/Vip.DynamicFilter/Extensions/QueryableExtensions.cs)
- [`ResponseExtensions`](../src/Vip.DynamicFilter/Extensions/ResponseExtensions.cs)
