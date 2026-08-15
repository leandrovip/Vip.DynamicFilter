# Ordenação (`Order` / `OrderBy` / `ThenBy`)

A ordenação é definida pela classe `Order`, com direção `OrderDirection` (`Asc` / `Desc`) ou string `Direction` (`"asc"` / `"desc"`).

| Classe | Propriedade | Descrição |
|---|---|---|
| `Order` | `Column` | Propriedade a ordenar (case-insensitive, aceita `.` para navegação) |
| | `DirectionType` | `OrderDirection.Asc` / `OrderDirection.Desc` |
| | `Direction` | String `"asc"` / `"desc"` |

## Ordenação simples

### C#

```csharp
var order = new Order("Age", OrderDirection.Desc);
var result = clients.OrderBy(order);
// clients.OrderByDescending(x => x.Age)
```

Também com string:

```csharp
var order = new Order { Column = "Age", Direction = "desc" };
var result = clients.OrderBy(order);
```

### JSON

```json
{
  "orderBy": [
    { "column": "age", "direction": "desc" }
  ]
}
```

## Ordenação múltipla (gera `ThenBy`)

### C#

```csharp
var orders = new List<Order>
{
    new("Age", OrderDirection.Desc),
    new("Name", OrderDirection.Desc)
};

var result = clients.OrderBy(orders);
// clients.OrderByDescending(x => x.Age).ThenByDescending(x => x.Name)
```

### JSON

```json
{
  "orderBy": [
    { "column": "age", "direction": "desc" },
    { "column": "name", "direction": "desc" }
  ]
}
```

## Combinando com filtro

A ordenação é aplicada **depois** do filtro dentro de `ApplyFilterRequest`:

```csharp
var request = new FilterRequest
{
    Where = new Filter
    {
        OperatorType = Operator.And,
        Filters = new List<Filter>
        {
            new("Age", WhereCondition.GreaterThanOrEqual, 15),
            new("Age", WhereCondition.LessThanOrEqual, 25)
        }
    },
    OrderBy = new List<Order>
    {
        new("Age", OrderDirection.Desc),
        new("Name", OrderDirection.Desc)
    }
};

// clients.Where(x => x.Age >= 15 && x.Age <= 25)
//        .OrderByDescending(x => x.Age)
//        .ThenByDescending(x => x.Name)
```

```json
{
  "where": {
    "operator": "and",
    "filters": [
      { "column": "age", "condition": ">=", "value": 15 },
      { "column": "age", "condition": "<=", "value": 25 }
    ]
  },
  "orderBy": [
    { "column": "age", "direction": "desc" },
    { "column": "name", "direction": "desc" }
  ]
}
```

## `ThenBy` explícito

É possível encadear manualmente usando `ThenBy`, desde que o resultado do `OrderBy` anterior seja `IOrderedQueryable<T>` (que é o retorno da própria extensão):

```csharp
var result = clients
    .OrderBy(new Order("Age", OrderDirection.Desc))
    .ThenBy(new Order("Name", OrderDirection.Desc));
```

## Ordenação por propriedade aninhada

A coluna aceita o separador `.`:

```csharp
var order = new Order("Address.Street", OrderDirection.Asc);
var result = clients.OrderBy(order);
// clients.OrderBy(x => x.Address.Street)
```

> Para coleções, a ordenação considera o primeiro elemento da navegação. Prefira ordenar por propriedades escalares no tipo raiz.

## Referência

- [`Order`](../src/Vip.DynamicFilter/Models/Order.cs)
- [`OrderDirection`](../src/Vip.DynamicFilter/Enums/OrderDirection.cs)
- [`QueryableExtensions`](../src/Vip.DynamicFilter/Extensions/QueryableExtensions.cs)
- [`OrderExpression`](../src/Vip.DynamicFilter/Expressions/OrderExpression.cs)
