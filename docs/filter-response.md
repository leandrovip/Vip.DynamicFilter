# Resposta paginada (`FilterResponse<T>`)

`FilterResponse<T>` encapsula o resultado de uma consulta filtrada, ordenada e paginada, incluindo o total de registros.

## Estrutura

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Data` | `IEnumerable<T>` | Itens da página atual |
| `TotalCount` | `int` | Total de registros **antes** da paginação |
| `PageNumber` | `int` | Página atual |
| `Limit` | `int` | Itens por página |

### Construtores

```csharp
public FilterResponse(IEnumerable<T> data, int totalCount, int pageNumber, int limit)
public FilterResponse(IEnumerable<T> data) // TotalCount = 0, PageNumber = 0, Limit = 0
```

## `GetFilterResponse`

A extensão `GetFilterResponse` aplica filtro, ordenação, conta os registros e então pagina:

```csharp
public static FilterResponse<T> GetFilterResponse<T>(this IQueryable<T> query, FilterRequest request) where T : class
```

### C#

```csharp
var request = new FilterRequest
{
    Where = new Filter("Age", WhereCondition.GreaterThanOrEqual, 18),
    OrderBy = new List<Order>
    {
        new("Name", OrderDirection.Asc)
    },
    PageNumber = 1,
    Limit = 10
};

var response = clients.GetFilterResponse(request);

var page = response.Data;      // IEnumerable<Client>
var total = response.TotalCount; // total de clientes com Age >= 18
```

### JSON

```json
{
  "where": { "column": "age", "condition": ">=", "value": 18 },
  "orderBy": [ { "column": "name", "direction": "asc" } ],
  "pageNumber": 1,
  "limit": 10
}
```

### Resposta típica (serializada)

```json
{
  "data": [ ... ],
  "totalCount": 123,
  "pageNumber": 1,
  "limit": 10
}
```

## Diferença entre `ApplyFilterRequest` e `GetFilterResponse`

| | `ApplyFilterRequest` | `GetFilterResponse` |
|---|---|---|
| Retorno | `IQueryable<T>` | `FilterResponse<T>` |
| Total de registros | — (não calcula) | Calcula `TotalCount` antes do `Skip`/`Take` |
| Materialização | Atrasada (deferred) | Imediata (`ToList`) |
| Restrição | Qualquer `T` | `T : class` |

## Uso em Minimal API

```csharp
app.MapPost("/clients", (FilterRequest filter) => clients.GetFilterResponse(filter));
```

```json
{
  "where": {
    "column": "name",
    "condition": "~~",
    "value": "batman"
  },
  "orderBy": [ { "column": "age", "direction": "desc" } ],
  "pageNumber": 1,
  "limit": 2
}
```

## Referência

- [`FilterResponse<T>`](../src/Vip.DynamicFilter/Responses/FilterResponse.cs)
- [`ResponseExtensions`](../src/Vip.DynamicFilter/Extensions/ResponseExtensions.cs)
