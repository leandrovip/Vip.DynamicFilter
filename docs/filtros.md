# Filtros (`Where` / `WhereCondition`)

Esta página documenta as **22 condições** disponíveis no enum `WhereCondition` e seus tokens JSON.

## Como declarar

### C# (enum)

```csharp
var filter = new Filter
{
    Column = "Name",
    ConditionType = WhereCondition.Contains,
    Value = "Silva"
};
```

### C# (string / factory)

```csharp
// A propriedade string "Condition" converte o token para o enum
var filter = new Filter { Column = "Name", Condition = "~", Value = "Silva" };

// Ou usando a factory estática de Where
var where = Where.New("Name", WhereCondition.Contains, "Silva");
```

### JSON

```json
{
  "where": {
    "column": "name",
    "condition": "~",
    "value": "Silva"
  }
}
```

## Tabela de condições

| Token | Enum | C# equivalente (LINQ) | Tipo |
|---|---|---|---|
| `=` | `Equal` | `x.Property == value` | |
| `!=` | `NotEqual` | `x.Property != value` | |
| `<` | `LessThan` | `x.Property < value` | |
| `>` | `GreaterThan` | `x.Property > value` | |
| `<=` | `LessThanOrEqual` | `x.Property <= value` | |
| `>=` | `GreaterThanOrEqual` | `x.Property >= value` | |
| `~` | `Contains` | `x.Property.Contains(value)` | string |
| `~~` | `ContainsIgnoreCase` | `x.Property.Contains(value, StringComparison.InvariantCultureIgnoreCase)` | string |
| `!~` | `NotContains` | `!x.Property.Contains(value)` | string |
| `*~` | `StartsWith` | `x.Property.StartsWith(value)` | string |
| `!*~` | `NotStartsWith` | `!x.Property.StartsWith(value)` | string |
| `~*` | `EndsWith` | `x.Property.EndsWith(value)` | string |
| `!~*` | `NotEndsWith` | `!x.Property.EndsWith(value)` | string |
| `any` | `Any` | `x.Collection.Any()` | coleção |
| `!any` | `NotAny` | `!x.Collection.Any()` | coleção |
| `isnull` | `IsNull` | `x.Property == null` | |
| `notnull` | `IsNotNull` | `x.Property != null` | |
| `isempty` | `IsEmpty` | `x.Property == ""` | string |
| `notempty` | `IsNotEmpty` | `x.Property != ""` | string |
| `isnullorempty` | `IsNullOrEmpty` | `x.Property == null \|\| x.Property == ""` | string |
| `notnullorempty` | `IsNotNullOrEmpty` | `!(x.Property == null \|\| x.Property == "")` | string |

> Condições de string (`isempty`, `notempty`, `isnullorempty`, `notnullorempty`) e as de substring (`~`, `~~`, `!~`, `*~`, `!*~`, `~*`, `!~*`) só podem ser aplicadas a propriedades do tipo `string`. Caso contrário é lançado `InvalidCastException`.

## Exemplos de cada condição

### `=` Igual

```csharp
var filter = new Filter("Name", WhereCondition.Equal, "Jose da Silva");
// clients.Where(x => x.Name == "Jose da Silva")
```

```json
{ "where": { "column": "name", "condition": "=", "value": "Jose da Silva" } }
```

### `!=` Diferente

```csharp
var filter = new Filter("Name", WhereCondition.NotEqual, "Jose da Silva");
// clients.Where(x => x.Name != "Jose da Silva")
```

```json
{ "where": { "column": "name", "condition": "!=", "value": "Jose da Silva" } }
```

### `<` Menor que

```csharp
var filter = new Filter("Age", WhereCondition.LessThan, 25);
// clients.Where(x => x.Age < 25)
```

```json
{ "where": { "column": "age", "condition": "<", "value": 25 } }
```

### `>` Maior que

```csharp
var filter = new Filter("Age", WhereCondition.GreaterThan, 25);
// clients.Where(x => x.Age > 25)
```

```json
{ "where": { "column": "age", "condition": ">", "value": 25 } }
```

### `<=` Menor ou igual

```csharp
var filter = new Filter("Age", WhereCondition.LessThanOrEqual, 25);
// clients.Where(x => x.Age <= 25)
```

```json
{ "where": { "column": "age", "condition": "<=", "value": 25 } }
```

### `>=` Maior ou igual

```csharp
var filter = new Filter("Age", WhereCondition.GreaterThanOrEqual, 14);
// clients.Where(x => x.Age >= 14)
```

```json
{ "where": { "column": "age", "condition": ">=", "value": 14 } }
```

### `~` Contains (case-sensitive)

```csharp
var filter = new Filter("Name", WhereCondition.Contains, "Silva");
// clients.Where(x => x.Name.Contains("Silva"))
```

```json
{ "where": { "column": "name", "condition": "~", "value": "Silva" } }
```

### `~~` ContainsIgnoreCase

```csharp
var filter = new Filter("Name", WhereCondition.ContainsIgnoreCase, "silva");
// clients.Where(x => x.Name.Contains("silva", StringComparison.InvariantCultureIgnoreCase))
```

```json
{ "where": { "column": "name", "condition": "~~", "value": "silva" } }
```

### `!~` NotContains

```csharp
var filter = new Filter("Name", WhereCondition.NotContains, "Silva");
// clients.Where(x => !x.Name.Contains("Silva"))
```

```json
{ "where": { "column": "name", "condition": "!~", "value": "Silva" } }
```

### `*~` StartsWith

```csharp
var filter = new Filter("Name", WhereCondition.StartsWith, "Jose");
// clients.Where(x => x.Name.StartsWith("Jose"))
```

```json
{ "where": { "column": "name", "condition": "*~", "value": "Jose" } }
```

### `!*~` NotStartsWith

```csharp
var filter = new Filter("Name", WhereCondition.NotStartsWith, "Jose");
// clients.Where(x => !x.Name.StartsWith("Jose"))
```

```json
{ "where": { "column": "name", "condition": "!*~", "value": "Jose" } }
```

### `~*` EndsWith

```csharp
var filter = new Filter("Name", WhereCondition.EndsWith, "Silva");
// clients.Where(x => x.Name.EndsWith("Silva"))
```

```json
{ "where": { "column": "name", "condition": "~*", "value": "Silva" } }
```

### `!~*` NotEndsWith

```csharp
var filter = new Filter("Name", WhereCondition.NotEndsWith, "Silva");
// clients.Where(x => !x.Name.EndsWith("Silva"))
```

```json
{ "where": { "column": "name", "condition": "!~*", "value": "Silva" } }
```

### `any` / `!any` (coleções)

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

### `isnull` / `notnull`

```csharp
var filter = new Filter("Name", WhereCondition.IsNull, null);
// clients.Where(x => x.Name == null)
```

```json
{ "where": { "column": "name", "condition": "isnull", "value": null } }
```

```csharp
var filter = new Filter("Name", WhereCondition.IsNotNull, null);
// clients.Where(x => x.Name != null)
```

```json
{ "where": { "column": "name", "condition": "notnull", "value": null } }
```

> Use preferencialmente em propriedades anuláveis (ex.: `string`). Em tipos não anuláveis a condição sempre será falsa.

### `isempty` / `notempty` (string)

```csharp
var filter = new Filter("Name", WhereCondition.IsEmpty, null);
// clients.Where(x => x.Name == "")
```

```json
{ "where": { "column": "name", "condition": "isempty", "value": null } }
```

```csharp
var filter = new Filter("Name", WhereCondition.IsNotEmpty, null);
// clients.Where(x => x.Name != "")
```

```json
{ "where": { "column": "name", "condition": "notempty", "value": null } }
```

### `isnullorempty` / `notnullorempty` (string)

```csharp
var filter = new Filter("Name", WhereCondition.IsNullOrEmpty, null);
// clients.Where(x => x.Name == null || x.Name == "")
```

```json
{ "where": { "column": "name", "condition": "isnullorempty", "value": null } }
```

```csharp
var filter = new Filter("Name", WhereCondition.IsNotNullOrEmpty, null);
// clients.Where(x => !(x.Name == null || x.Name == ""))
```

```json
{ "where": { "column": "name", "condition": "notnullorempty", "value": null } }
```

## Aplicando um filtro `Where` direto

O filtro também pode ser aplicado isoladamente, sem `FilterRequest`:

```csharp
var where = Where.New("Age", WhereCondition.GreaterThanOrEqual, 35);
var result = clients.Where(where);

// Ou via Filter (sem operador)
var filter = new Filter("Age", WhereCondition.GreaterThanOrEqual, 35);
var result2 = clients.Where(filter);
```

## Referência

- [`Where`](../src/Vip.DynamicFilter/Models/Where.cs)
- [`WhereCondition`](../src/Vip.DynamicFilter/Enums/WhereCondition.cs)
- [`WhereExpression`](../src/Vip.DynamicFilter/Expressions/WhereExpression.cs)
