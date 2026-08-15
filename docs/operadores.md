# Operadores (`AND` / `OR`)

Filtros compostos combinam várias condições. O operador é definido em `Filter.OperatorType` (enum `Operator`) ou pela string `Filter.Operator` (`"and"`, `"or"`, `"none"`).

| Token | Enum `Operator` | Comportamento |
|---|---|---|
| `none` | `None` | O `Filter` é tratado como **condição única** (usa `Column`/`ConditionType`/`Value` do próprio filtro) |
| `and` | `And` | Combina `Filters` com `&&` |
| `or` | `Or` | Combina `Filters` com `\|\|` |

> **Importante**: quando o operador é `And`/`Or`, a lista `Filters` é **obrigatória** e não pode ser vazia (`ArgumentException`). As propriedades `Column`/`Value` do filtro pai são ignoradas nesse caso.

## Filtro com `AND`

### C#

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

// clients.Where(x => x.Age >= 14 && x.Age <= 25)
```

### JSON

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

Também pode usar a string: `new Filter { Operator = "and", Filters = ... }`.

## Filtro com `OR`

### C#

```csharp
var request = new FilterRequest
{
    Where = new Filter
    {
        OperatorType = Operator.Or,
        Filters = new List<Filter>
        {
            new("Name", WhereCondition.ContainsIgnoreCase, "batman"),
            new("Name", WhereCondition.Equal, "Joao")
        }
    }
};

// clients.Where(x => x.Name.Contains("batman", StringComparison.InvariantCultureIgnoreCase) || x.Name == "Joao")
```

### JSON

```json
{
  "where": {
    "operator": "or",
    "filters": [
      { "column": "name", "condition": "~~", "value": "batman" },
      { "column": "name", "condition": "=", "value": "Joao" }
    ]
  }
}
```

## Aninhamento

Filtros podem conter filtros, em quantos níveis forem necessários. Cada nível tem seu próprio operador.

### C#

```csharp
var request = new FilterRequest
{
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
    }
};

// clients.Where(x => x.Name.Contains("Silva") && (x.Age == 22 || x.Age == 25))
```

### JSON

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

## Operador `none`

Se `OperatorType` for `None` (default), o `Filter` é tratado como condição única:

```csharp
var request = new FilterRequest
{
    Where = new Filter
    {
        Column = "Age",
        ConditionType = WhereCondition.GreaterThanOrEqual,
        Value = 56
    }
};

// clients.Where(x => x.Age >= 56)
```

```json
{
  "where": {
    "column": "age",
    "condition": ">=",
    "value": 56
  }
}
```

## Precedência

Não há precedência implícita: a árvore é avaliada **na ordem declarada**, respeitando os parênteses criados pelo aninhamento explícito dos `Filters`.

## Referência

- [`Operator`](../src/Vip.DynamicFilter/Enums/Operator.cs)
- [`Filter`](../src/Vip.DynamicFilter/Models/Filter.cs)
- [`WhereExpression.GetFilterExpression`](../src/Vip.DynamicFilter/Expressions/WhereExpression.cs)
