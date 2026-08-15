# Propriedades aninhadas e coleções

A `Column` aceita o separador `.` para navegar por propriedades de navegação. Quando a navegação é uma **coleção**, a condição é aplicada via `Any(...)`.

## Navegação por propriedade única

```csharp
// Property "Address" é ICollection<Address>
var filter = new Filter("Address.Street", WhereCondition.Contains, "Rua das Palmeiras");
// clients.Where(x => x.Address.Any(a => a.Street.Contains("Rua das Palmeiras")))
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

## Combinando navegação com outras condições

```csharp
var request = new FilterRequest
{
    Where = new Filter
    {
        OperatorType = Operator.And,
        Filters = new List<Filter>
        {
            new("Age", WhereCondition.GreaterThanOrEqual, 5),
            new("Address.Street", WhereCondition.Contains, "Rua das Palmeiras")
        }
    }
};

// clients.Where(x => x.Age >= 5 && x.Address.Any(a => a.Street.Contains("Rua das Palmeiras")))
```

```json
{
  "where": {
    "operator": "and",
    "filters": [
      { "column": "age", "condition": ">=", "value": 5 },
      { "column": "address.street", "condition": "~", "value": "Rua das Palmeiras" }
    ]
  }
}
```

## Navegação com case-insensitive

```csharp
var filter = new Filter("Address.Street", WhereCondition.ContainsIgnoreCase, "rua das palmeiras");
// clients.Where(x => x.Address.Any(a => a.Street.Contains("rua das palmeiras", StringComparison.InvariantCultureIgnoreCase)))
```

```json
{
  "where": {
    "column": "address.street",
    "condition": "~~",
    "value": "rua das palmeiras"
  }
}
```

## Verificando se a coleção possui elementos (`any` / `!any`)

Aplique a condição diretamente sobre a **propriedade de coleção**:

```csharp
var hasAddress = new Filter("Address", WhereCondition.Any, null);
// clients.Where(x => x.Address.Any())

var noAddress = new Filter("Address", WhereCondition.NotAny, null);
// clients.Where(x => !x.Address.Any())
```

```json
{ "where": { "column": "address", "condition": "any", "value": null } }
```

```json
{ "where": { "column": "address", "condition": "!any", "value": null } }
```

> O `value` é ignorado nessas condições.

## Limitações da navegação

- A navegação em **coleção** aplica `Any()` sobre a condição informada no **último segmento** da `Column`. Um nível de aninhamento abaixo da coleção é o cenário suportado/testado (`Address.Street`).
- Para propriedades de coleção com `any`/`!any`, a propriedade deve ser `IEnumerable<T>` (com tipo genérico).
- Propriedade inexistente no tipo alvo lança `InvalidOperationException`.

## Referência

- [`WhereExpression.GetExpressionForColumn`](../src/Vip.DynamicFilter/Expressions/WhereExpression.cs)
- [filtros.md](filtros.md) — condições `any` / `!any`
