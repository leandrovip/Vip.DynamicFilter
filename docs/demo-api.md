# DemoApi e coleção Postman

O projeto [`demo/Vip.DynamicFilter.DemoApi`](../demo/Vip.DynamicFilter.DemoApi) é uma Minimal API (ASP.NET Core, `net10.0`) que expõe a biblioteca sobre dados em memória. Serve para experimentar os payloads JSON; não usa banco de dados.

## Como executar

```bash
dotnet run --project ./demo/Vip.DynamicFilter.DemoApi --launch-profile https
```

| Perfil (`launchSettings.json`) | URLs |
|---|---|
| `http` | `http://localhost:5071` |
| `https` | `https://localhost:7165` e `http://localhost:5071` |
| `IIS Express` | `http://localhost:38727` / SSL `44358` |

- O Swagger UI fica em `/swagger` e só é habilitado em `Development` (padrão dos perfis acima).
- `UseHttpsRedirection` está ativo; a coleção Postman aponta para `https://localhost:7165`, então use o perfil `https` (e confie no certificado de desenvolvimento: `dotnet dev-certs https --trust`).
- Requer o SDK/runtime .NET 10. O documento OpenAPI e a interface são gerados pelo Swashbuckle (`AddSwaggerGen` / `UseSwaggerUI`); as rotas não usam `WithOpenApi()`, descontinuado no .NET 10.

## Endpoints

| Método | Rota | Corpo | Retorno | Extensão usada |
|---|---|---|---|---|
| `GET` | `/` | — | texto de boas-vindas | — |
| `GET` | `/clients` | — | todos os clientes | — |
| `GET` | `/clients/{id:guid}` | — | cliente (ou vazio se não existir) | — |
| `POST` | `/clients` | `FilterRequest` | `FilterResponse<Client>` | `GetFilterResponse` |
| `POST` | `/clients/query` | `FilterRequest` | `List<Client>` | `ApplyFilterRequest(...).ToList()` |
| `POST` | `/addresses` | `FilterRequest` | `FilterResponse<Address>` | `GetFilterResponse` |

A diferença entre os dois `POST` de clientes é exatamente a diferença entre as extensões: o primeiro devolve o envelope com `totalCount`; o segundo, só a lista.

A API não trata exceções: um filtro inválido (coluna inexistente, token desconhecido, valor não conversível) responde HTTP 500. Isso é proposital para manter o exemplo mínimo — veja [seguranca-e-performance.md](seguranca-e-performance.md) antes de copiar o padrão para produção.

## Configuração de JSON

```csharp
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
```

Com isso, enums saem como texto (`"clientType": "Servico"`) e `conditionType`/`operatorType`/`directionType` também aceitam o nome do enum.

## Dados (`Data/Seed.cs`)

- **15 clientes** fixos: os mesmos 14 do mock de testes mais **"Sem Endereço"** (idade 30, lista de endereços vazia), que existe para demonstrar `any` / `!any`.
- Cada cliente com rua recebe **3 endereços** (`"<rua> 1"`, `"<rua> 2"`, `"<rua> 3"`), todos com `city = "São Paulo"` e `zipCode` igual à idade → **42 endereços** em `/addresses`.
- `birthday` = data atual menos `age` anos.
- `clientId` e `addressId` são `Guid.NewGuid()`: **mudam a cada inicialização**.

## Exemplo

```csharp
// Equivalente em C# ao request abaixo
var request = new FilterRequest
{
    Where = new Filter
    {
        OperatorType = Operator.And,
        Filters = new List<Filter>
        {
            new("ClientType", WhereCondition.Equal, "Especial"),
            new("Address", WhereCondition.Any, null)
        }
    },
    OrderBy = new List<Order> { new("Age", OrderDirection.Desc) },
    PageNumber = 1,
    Limit = 2
};
var response = Seed.Clients.GetFilterResponse(request);
```

```json
{
  "where": {
    "operator": "and",
    "filters": [
      { "column": "clientType", "condition": "=", "value": "Especial" },
      { "column": "address", "condition": "any" }
    ]
  },
  "orderBy": [ { "column": "age", "direction": "desc" } ],
  "pageNumber": 1,
  "limit": 2
}
```

`POST /clients` devolve os dois clientes `Especial` mais velhos (78 e 65 anos) com `totalCount: 5`.

## Coleção Postman

[`collections/postman.json`](../collections/postman.json) — coleção "Vip.DynamicFilter" com **28 requests**, todos `POST`, sem pastas e com a URL base fixa `https://localhost:7165` (não há variável de coleção).

| Grupo | Requests |
|---|---|
| Básico | All Clients · Filter by Name · Filter by Exact Name · Filter by Id |
| Operadores | Filter by Name and Age · Filter by Name or Name · Filter by Age Range · Nested And Or |
| Condições de texto | Filter by Name StartsWith · Filter by Name EndsWith · Filter by Name NotContains · Filter by Name NotNull |
| Tipos | Filter by Enum ClientType · Filter by Birthday Before |
| `condition` por nome do enum | Filter by Condition Enum Name · Filter by Birthday Range (Enum Name) |
| Erro | Invalid Condition (error) |
| Coleções | Filter by Nested Address · Filter by Nested Address IgnoreCase · Filter by Client with Address · Filter by Client without Address |
| Ordenação e paginação | Order By Age · Order By Multiple · Paging One · Paging Two · Full Request |
| Outros endpoints | Query ApplyFilterRequest (`/clients/query`) · Filter Addresses (`/addresses`) |

Observações:

- **Filter by Id** precisa de um `clientId` atual: obtenha um em `GET /clients` (os ids mudam a cada inicialização).
- **Filter by Name or Name** filtra `name ~~ "batman" OR name = "Joao"`, valores que não existem no seed — o resultado vazio é esperado.
- **Filter by Condition Enum Name** e **Filter by Birthday Range (Enum Name)** usam `"condition": "GreaterThanOrEqual"` / `"LessThan"` no lugar dos tokens, com `operator` e `direction` em maiúsculas/minúsculas variadas. O intervalo de datas usa limite superior exclusivo (`< "2011-01-01"`) para não perder registros com horário no último dia.
- **Invalid Condition (error)** envia `"condition": "maiorque"` e responde HTTP 500 com `ArgumentException: Condition 'maiorque' is not recognized for column 'age'.` — a demo não trata exceções.
- As rotas `GET` não estão na coleção; use o Swagger.

## Referência

- [`Program.cs`](../demo/Vip.DynamicFilter.DemoApi/Program.cs)
- [`Data/Seed.cs`](../demo/Vip.DynamicFilter.DemoApi/Data/Seed.cs)
- [`Properties/launchSettings.json`](../demo/Vip.DynamicFilter.DemoApi/Properties/launchSettings.json)
