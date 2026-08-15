
<h2 align="center"><strong>Vip.DynamicFilter</strong></h2>

<p align="center">
  <a href="https://raw.githubusercontent.com/leandrovip/Vip.DynamicFilter/master/LICENSE">
    <img src="https://img.shields.io/github/license/leandrovip/Vip.DynamicFilter" />
  </a>

  <a href="https://www.nuget.org/packages/Vip.DynamicFilter/">
    <img alt="Nuget" src="https://img.shields.io/nuget/dt/Vip.DynamicFilter?label=NuGet%20downloads&style=flat-square">
  </a>

  <a href="https://www.nuget.org/packages/Vip.DynamicFilter/">
     <img alt="NuGet" src="https://img.shields.io/nuget/v/Vip.DynamicFilter.svg">
  </a>
</p>

Biblioteca responsável por gerar **filtros dinâmicos** utilizando **expression trees**, destinada a **.NET Standard 2.0**.

Com ela você pode aplicar filtros, ordenação e paginação em qualquer `IQueryable<T>` (EF Core, LINQ to SQL, coleções em memória) **sem escrever código condicional**, recebendo os critérios como objetos C# ou diretamente de um **JSON** (ex.: corpo de uma requisição HTTP).

> Em desenvolvimento, use por sua conta e risco.

## Funcionalidades

- **Filtros simples** com mais de 20 condições (`=`, `!=`, `>`, `>=`, `<`, `<=`, `~`, `~~`, ...).
- **Filtros compostos** com operadores `AND` / `OR`, suportando aninhamento em vários níveis.
- **Ordenação** simples ou múltipla (ascendente / descendente).
- **Paginação** (`PageNumber` + `Limit`) e resposta paginada com total de registros.
- **Propriedades aninhadas e coleções**: filtre por `Address.Street` (gera `Any(...)`) e use `any` / `!any`.
- **Tipagem automática**: valores são convertidos para o tipo da propriedade (int, decimal, DateTime, Guid, bool, enum, string, ...).
- **Deserialização de JSON**: receba o filtro pronto de `System.Text.Json` ou `Newtonsoft.Json`.
- Compatível com `IQueryable<T>` e `IEnumerable<T>`.

## Pré-requisitos

- .NET Standard 2.0

## Instalação via NuGet

```powershell
Install-Package Vip.DynamicFilter
```

ou

```bash
dotnet add package Vip.DynamicFilter
```

---

## Modelo de exemplo

Todos os exemplos desta documentação usam o modelo abaixo (mesmo dos testes da biblioteca):

```csharp
public class Client
{
    public Guid ClientId { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime Birthday { get; set; }
    public ClientEnum ClientType { get; set; }
    public ICollection<Address> Address { get; set; }
}

public class Address
{
    public Guid AddressId { get; set; }
    public Guid ClientId { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public int ZipCode { get; set; }
}

public enum ClientEnum
{
    Padrao = 0,
    Especial = 1,
    Servico = 2
}
```

---

## Quick Start

### Com objetos C#

```csharp
using Vip.DynamicFilter;

var request = new FilterRequest
{
    Where = new Filter
    {
        Column = "Name",
        ConditionType = WhereCondition.Contains,
        Value = "Silva"
    },
    OrderBy = new List<Order>
    {
        new("Age", OrderDirection.Desc)
    },
    PageNumber = 1,
    Limit = 10
};

var clients = _context.Clients.ApplyFilterRequest(request).ToList();
```

### Com JSON (via API)

```json
{
  "where": {
    "column": "name",
    "condition": "~",
    "value": "Silva"
  },
  "orderBy": [
    { "column": "age", "direction": "desc" }
  ],
  "pageNumber": 1,
  "limit": 10
}
```

```csharp
var request = JsonSerializer.Deserialize<FilterRequest>(jsonBody, JsonOptions.Default);
var clients = _context.Clients.ApplyFilterRequest(request).ToList();
```

### Deserialização de JSON (importante)

Para que os **tokens** (`condition`, `operator`, `direction`) sejam convertidos para os enums automaticamente, configure a serialização com nomes **camelCase**. O enum é opcional, mas recomendado para compatibilidade total:

```csharp
// System.Text.Json
public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
}

var request = JsonSerializer.Deserialize<FilterRequest>(jsonBody, JsonOptions.Default);
```

```csharp
// Newtonsoft.Json
var settings = new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver()
};

var request = JsonConvert.DeserializeObject<FilterRequest>(jsonBody, settings);
```

> Veja a página [`docs/serializacao-json.md`](docs/serializacao-json.md) para os detalhes completos.

---

## Como usar

### 1. Filtro simples (`Where`)

Um filtro sem operador é tratado como condição única. Use `Filter` (recomendado) ou `Where`:

```csharp
var request = new FilterRequest
{
    Where = new Filter
    {
        Column = "Name",
        ConditionType = WhereCondition.NotEqual,
        Value = "Jose da Silva"
    }
};

var result = clients.ApplyFilterRequest(request);
// Equivale a: clients.Where(x => x.Name != "Jose da Silva")
```

```json
{
  "where": {
    "column": "name",
    "condition": "!=",
    "value": "Jose da Silva"
  }
}
```

### 2. Condições disponíveis (`WhereCondition`)

| Token (JSON) | Enum C# | Descrição | Restrito a |
|---|---|---|---|
| `=` | `WhereCondition.Equal` | Igual | |
| `!=` | `WhereCondition.NotEqual` | Diferente | |
| `<` | `WhereCondition.LessThan` | Menor que | |
| `>` | `WhereCondition.GreaterThan` | Maior que | |
| `<=` | `WhereCondition.LessThanOrEqual` | Menor ou igual | |
| `>=` | `WhereCondition.GreaterThanOrEqual` | Maior ou igual | |
| `~` | `WhereCondition.Contains` | Contém (case-sensitive) | string |
| `~~` | `WhereCondition.ContainsIgnoreCase` | Contém (ignora caixa) | string |
| `!~` | `WhereCondition.NotContains` | Não contém | string |
| `*~` | `WhereCondition.StartsWith` | Começa com | string |
| `!*~` | `WhereCondition.NotStartsWith` | Não começa com | string |
| `~*` | `WhereCondition.EndsWith` | Termina com | string |
| `!~*` | `WhereCondition.NotEndsWith` | Não termina com | string |
| `any` | `WhereCondition.Any` | Coleção possui elementos | coleção |
| `!any` | `WhereCondition.NotAny` | Coleção vazia | coleção |
| `isnull` | `WhereCondition.IsNull` | É nulo | |
| `notnull` | `WhereCondition.IsNotNull` | Não é nulo | |
| `isempty` | `WhereCondition.IsEmpty` | String vazia (`""`) | string |
| `notempty` | `WhereCondition.IsNotEmpty` | String não vazia | string |
| `isnullorempty` | `WhereCondition.IsNullOrEmpty` | Nula ou vazia | string |
| `notnullorempty` | `WhereCondition.IsNotNullOrEmpty` | Não nula e não vazia | string |

Exemplos de cada condição em C# e JSON na página [`docs/filtros.md`](docs/filtros.md).

### 3. Filtros compostos (`AND` / `OR`)

Para combinar condições, defina `OperatorType` (`Operator.And` / `Operator.Or`) e preencha `Filters`:

```csharp
var request = new FilterRequest
{
    Where = new Filter
    {
        OperatorType = Operator.And,
        Filters = new List<Filter>
        {
            new("Age", WhereCondition.GreaterThanOrEqual, 14),
            new("Age", WhereCondition.LessThanOrEqual, 25)
        }
    }
};

// Equivale a: clients.Where(x => x.Age >= 14 && x.Age <= 25)
```

```json
{
  "where": {
    "operator": "and",
    "filters": [
      { "column": "age", "condition": ">=", "value": 14 },
      { "column": "age", "condition": "<=", "value": 25 }
    ]
  }
}
```

**Aninhamento** (filtro dentro de filtro):

```csharp
Where = new Filter
{
    OperatorType = Operator.And,
    Filters = new List<Filter>
    {
        new("Name", WhereCondition.Contains, "Silva"),
        new Filter
        {
            OperatorType = Operator.Or,
            Filters = new List<Filter>
            {
                new("Age", WhereCondition.Equal, 22),
                new("Age", WhereCondition.Equal, 25)
            }
        }
    }
};

// Equivale a: clients.Where(x => x.Name.Contains("Silva") && (x.Age == 22 || x.Age == 25))
```

```json
{
  "where": {
    "operator": "and",
    "filters": [
      { "column": "name", "condition": "~", "value": "Silva" },
      {
        "operator": "or",
        "filters": [
          { "column": "age", "condition": "=", "value": 22 },
          { "column": "age", "condition": "=", "value": 25 }
        ]
      }
    ]
  }
}
```

### 4. Ordenação

Simples:

```csharp
var order = new Order("Age", OrderDirection.Desc);
var result = clients.OrderBy(order);
// Equivale a: clients.OrderByDescending(x => x.Age)
```

```json
{ "orderBy": [ { "column": "age", "direction": "desc" } ] }
```

Múltipla (gera `ThenBy`):

```csharp
var orders = new List<Order>
{
    new("Age", OrderDirection.Desc),
    new("Name", OrderDirection.Desc)
};
var result = clients.OrderBy(orders);
// Equivale a: clients.OrderByDescending(x => x.Age).ThenByDescending(x => x.Name)
```

```json
{
  "orderBy": [
    { "column": "age", "direction": "desc" },
    { "column": "name", "direction": "desc" }
  ]
}
```

Também é possível usar `ThenBy` diretamente sobre um `OrderBy` prévio.

### 5. Paginação

`PageNumber` é **1-based**. A paginação é aplicada **somente** quando `Limit > 0` (o default de `Limit` é `-1`, ou seja, sem paginação).

```csharp
var request = new FilterRequest
{
    PageNumber = 2,
    Limit = 2
};
var result = clients.ApplyFilterRequest(request);
// Equivale a: clients.Skip((2 - 1) * 2).Take(2)
```

```json
{ "pageNumber": 2, "limit": 2 }
```

### 6. Resposta paginada (`FilterResponse<T>`)

Para obter a lista **e** o total de registros sem paginação, use `GetFilterResponse`:

```csharp
var response = clients.GetFilterResponse(request);
// response.Data        → IEnumerable<Client>
// response.TotalCount  → int (total de registros ANTES da paginação)
// response.PageNumber  → int
// response.Limit       → int
```

```json
{
  "where": { "column": "age", "condition": ">=", "value": 18 },
  "orderBy": [ { "column": "name", "direction": "asc" } ],
  "pageNumber": 1,
  "limit": 10
}
```

### 7. Propriedades aninhadas e coleções

Use o separador `.` para navegar. Em coleções, a condição é aplicada via `Any(...)`:

```csharp
var request = new FilterRequest
{
    Where = new Filter
    {
        Column = "Address.Street",
        ConditionType = WhereCondition.Contains,
        Value = "Rua das Palmeiras"
    }
};

// Equivale a: clients.Where(x => x.Address.Any(a => a.Street.Contains("Rua das Palmeiras")))
```

```json
{
  "where": {
    "column": "address.street",
    "condition": "~",
    "value": "Rua das Palmeiras"
  }
}
```

Para verificar se uma coleção possui (ou não) elementos:

```csharp
new("Address", WhereCondition.Any, null)    // clients.Where(x => x.Address.Any())
new("Address", WhereCondition.NotAny, null) // clients.Where(x => !x.Address.Any())
```

### 8. Enums e conversão automática de tipos

O `value` é convertido automaticamente para o tipo da propriedade:

```csharp
// Enum (string com o nome do membro)
new("ClientType", WhereCondition.Equal, "Servico")
// Equivale a: clients.Where(x => x.ClientType == ClientEnum.Servico)

// Numérico
new("Age", WhereCondition.GreaterThanOrEqual, "14")

// DateTime
new("Birthday", WhereCondition.LessThan, "1990-01-01")
```

```json
{
  "where": {
    "column": "clientType",
    "condition": "=",
    "value": "Servico"
  }
}
```

---

## Referência da API

| Classe | Propriedade | Descrição |
|---|---|---|
| `FilterRequest` | `Where` (`Filter`) | Filtro (simples ou composto) |
| | `OrderBy` (`List<Order>`) | Ordenações (aplica `OrderBy` + `ThenBy`) |
| | `PageNumber` (`int`) | Página (1-based), default `0` |
| | `Limit` (`int`) | Itens por página, default `-1` (sem limite) |
| `Where` | `Column` (`string`) | Nome da propriedade (case-insensitive, aceita `.`) |
| | `ConditionType` (`WhereCondition`) | Condição via enum |
| | `Value` (`object`) | Valor comparado |
| | `Condition` (`string`) | Condição via token (`"="`, `"~~"`, ...) |
| | `Where.New(column, condition, value)` | Factory estática |
| `Filter : Where` | `OperatorType` (`Operator`) | `None`, `And` ou `Or` |
| | `Operator` (`string`) | `"and"`, `"or"` ou `"none"` |
| | `Filters` (`List<Filter>`) | Subfiltros (obrigatório quando operador ≠ `None`) |
| `Order` | `Column` (`string`) | Propriedade a ordenar |
| | `DirectionType` (`OrderDirection`) | `Asc` ou `Desc` |
| | `Direction` (`string`) | `"asc"` ou `"desc"` |
| `FilterResponse<T>` | `Data`, `TotalCount`, `PageNumber`, `Limit` | Resultado paginado |

### Extensões

```csharp
// QueryableExtensions
query.ApplyFilterRequest(request)                     // IQueryable<T> / IEnumerable<T>
query.Where(where)                                    // Where ou Filter
query.OrderBy(order)                                  // Order ou IEnumerable<Order>
query.OrderBy(orders)
query.ThenBy(order)

// ResponseExtensions
query.GetFilterResponse(request)                      // retorna FilterResponse<T>
```

---

## Comportamentos e limitações

- **Nome de propriedade**: case-insensitive (`name` = `Name`). Propriedade inexistente lança `InvalidOperationException`.
- **Operador ≠ `None` exige filtros**: `Filters` vazio lança `ArgumentException`.
- **Conversão de valor**: incompatível com o tipo da propriedade lança `InvalidCastException`.
- **Condições de string** (`isempty`, `notempty`, `isnullorempty`, `notnullorempty`, `~`, `~~`, ...): aplicáveis apenas a `string`, caso contrário `InvalidCastException`.
- **`any` / `!any`**: exigem propriedade de coleção (`IEnumerable<T>`).
- **`Contains` (`~`)** é case-sensitive; para ignorar caixa use `ContainsIgnoreCase` (`~~`).
- **Nulos**: `Where`/`OrderBy` nulos são ignorados sem erro.
- **Paginação**: `ApplyFilterRequest` aplica `Skip` apenas se `PageNumber > 0 && Limit > 0`, e `Take` apenas se `Limit > 0`.

---

## Exemplo completo (Minimal API)

O projeto [`demo/`](demo/Vip.DynamicFilter.DemoApi) contém uma Minimal API completa. Resumo:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Vip.DynamicFilter;

var builder = WebApplication.CreateBuilder(args);

// camelCase + enums como string (recomendado para os tokens do DynamicFilter)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

var clients = Seed.Clients;       // List<Client> com endereços, enums e datas
var addresses = Seed.Addresses;   // List<Address> (para demonstrar outro tipo)

app.MapGet("/clients", () => clients).WithOpenApi();
app.MapGet("/clients/{id:guid}", (Guid id) => clients.FirstOrDefault(x => x.ClientId == id)).WithOpenApi();

// Retorna FilterResponse<T> (com total de registros)
app.MapPost("/clients", (FilterRequest filter) => clients.GetFilterResponse(filter)).WithOpenApi();

// Retorna apenas a lista materializada
app.MapPost("/clients/query", (FilterRequest filter) => clients.ApplyFilterRequest(filter).ToList()).WithOpenApi();

// Filtro sobre outro tipo (Address)
app.MapPost("/addresses", (FilterRequest filter) => addresses.GetFilterResponse(filter)).WithOpenApi();

app.Run();
```

> Uma coleção Postman pronta para testar todos os cenários está em [`collections/postman.json`](collections/postman.json).

## Documentação

- [`docs/README.md`](docs/README.md) — índice da documentação.
- [`docs/filtros.md`](docs/filtros.md) — todas as condições com exemplos C# + JSON.
- [`docs/operadores.md`](docs/operadores.md) — AND/OR e filtros aninhados.
- [`docs/ordenacao.md`](docs/ordenacao.md) — ordenação simples e múltipla.
- [`docs/paginacao.md`](docs/paginacao.md) — paginação e semântica.
- [`docs/filter-response.md`](docs/filter-response.md) — resposta paginada.
- [`docs/propriedades-aninhadas.md`](docs/propriedades-aninhadas.md) — navegação e coleções.
- [`docs/enums-e-tipos.md`](docs/enums-e-tipos.md) — enums e conversão de tipos.
- [`docs/serializacao-json.md`](docs/serializacao-json.md) — deserialização JSON.
- [`docs/ef-core.md`](docs/ef-core.md) — uso com EF Core.
- [`docs/limites-e-comportamentos.md`](docs/limites-e-comportamentos.md) — exceções e edge cases.

## Licença

MIT
