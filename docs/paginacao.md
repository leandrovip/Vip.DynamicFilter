# Paginação (`PageNumber` / `Limit`)

A paginação usa `PageNumber` (1-based) e `Limit` (itens por página) do `FilterRequest`.

| Propriedade | Default | Comportamento |
|---|---|---|
| `PageNumber` | `0` | Página atual (a primeira página é `1`) |
| `Limit` | `-1` | Itens por página. `<= 0` significa **sem paginação** |

## Aplicando paginação

### C#

```csharp
var request = new FilterRequest
{
    PageNumber = 2,
    Limit = 2
};

var result = clients.ApplyFilterRequest(request);
// clients.Skip((2 - 1) * 2).Take(2)
```

### JSON

```json
{
  "pageNumber": 2,
  "limit": 2
}
```

## Semântica de paginação

### `ApplyFilterRequest`

```csharp
if (request.PageNumber > 0 && request.Limit > 0)
    res = res.Skip((request.PageNumber - 1) * request.Limit);

if (request.Limit > 0)
    res = res.Take(request.Limit);
```

- `Skip` é aplicado **somente** quando `PageNumber > 0` **e** `Limit > 0`.
- `Take` é aplicado quando `Limit > 0`.
- Com o default `Limit = -1`, a query retorna **todos** os registros.

### `GetFilterResponse`

```csharp
var count = result.Count();            // total ANTES da paginação
if (request.PageNumber > 0) result = result.Skip((request.PageNumber - 1) * request.Limit);
if (request.Limit > 0) result = result.Take(request.Limit);
```

- O `TotalCount` é calculado **antes** do `Skip`/`Take`.
- Recomenda-se sempre informar `PageNumber` **e** `Limit` juntos para evitar `Skip` com `Limit` negativo (default).

## Exemplos combinados

Filtro + ordenação + paginação:

```csharp
var request = new FilterRequest
{
    Where = new Filter
    {
        OperatorType = Operator.And,
        Filters = new List<Filter>
        {
            new("Age", WhereCondition.GreaterThanOrEqual, 35),
            new("Age", WhereCondition.LessThanOrEqual, 56)
        }
    },
    OrderBy = new List<Order>
    {
        new("Name", OrderDirection.Desc)
    },
    PageNumber = 2,
    Limit = 2
};

// clients.Where(x => x.Age >= 35 && x.Age <= 56)
//        .OrderByDescending(x => x.Name)
//        .Skip(1 * 2)
//        .Take(2)
```

```json
{
  "where": {
    "operator": "and",
    "filters": [
      { "column": "age", "condition": ">=", "value": 35 },
      { "column": "age", "condition": "<=", "value": 56 }
    ]
  },
  "orderBy": [
    { "column": "name", "direction": "desc" }
  ],
  "pageNumber": 2,
  "limit": 2
}
```

## Sem paginação (todos os registros)

Omita `pageNumber`/`limit` (ou informe `limit <= 0`):

```json
{
  "where": { "column": "age", "condition": ">=", "value": 18 }
}
```

```csharp
var request = new FilterRequest
{
    Where = new Filter("Age", WhereCondition.GreaterThanOrEqual, 18)
};
var all = clients.ApplyFilterRequest(request);
```

## Referência

- [`FilterRequest`](../src/Vip.DynamicFilter/Requests/FilterRequest.cs)
- [`QueryableExtensions.ApplyFilterRequest`](../src/Vip.DynamicFilter/Extensions/QueryableExtensions.cs)
- [`ResponseExtensions.GetFilterResponse`](../src/Vip.DynamicFilter/Extensions/ResponseExtensions.cs)
