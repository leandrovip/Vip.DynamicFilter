# Enums e conversão automática de tipos

O valor informado em `Value` é convertido automaticamente para o tipo da propriedade (`TryCastColumnValueType`). Isso permite enviar valores como **string** (muito comum vindo de JSON) e deixar a biblioteca cuidar da conversão.

## Tipos suportados

| Categoria | Tipos |
|---|---|
| Texto | `string` |
| Numéricos | `byte?`, `sbyte?`, `short`, `short?`, `ushort`, `ushort?`, `int`, `int?`, `uint`, `uint?`, `long`, `long?`, `ulong`, `ulong?`, `float`, `float?`, `double`, `double?`, `decimal`, `decimal?`, `char`, `char?` |
| Data/hora | `DateTime`, `DateTime?`, `DateTimeOffset`, `DateTimeOffset?`, `TimeSpan`, `TimeSpan?` |
| Outros | `bool`, `bool?`, `Guid`, `Guid?`, enums |

> Se o tipo não for suportado (ou o valor for incompatível), é lançado `InvalidCastException`.

## Exemplos

### Enum (nome do membro)

```csharp
var filter = new Filter("ClientType", WhereCondition.Equal, "Servico");
// clients.Where(x => x.ClientType == ClientEnum.Servico)
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

> Para enums o valor é convertido via `Enum.Parse`. Envie o **nome do membro** (ex.: `"Servico"`) ou o valor numérico (ex.: `2`).

### Número como string

```csharp
var filter = new Filter("Age", WhereCondition.Equal, "22");
// clients.Where(x => x.Age == 22)
```

```json
{ "where": { "column": "age", "condition": "=", "value": "22" } }
```

### DateTime

```csharp
var filter = new Filter("Birthday", WhereCondition.LessThan, "1990-01-01");
// clients.Where(x => x.Birthday < new DateTime(1990, 1, 1))
```

```json
{ "where": { "column": "birthday", "condition": "<", "value": "1990-01-01" } }
```

### Guid

```csharp
var id = Guid.NewGuid();
var filter = new Filter("ClientId", WhereCondition.Equal, id.ToString());
// clients.Where(x => x.ClientId == id)
```

```json
{
  "where": {
    "column": "clientId",
    "condition": "=",
    "value": "3f2504e0-4f89-41d3-9a0c-0305e82c3301"
  }
}
```

### bool

Supondo uma propriedade `bool IsActive` no modelo (não presente no `Client` de exemplo):

```csharp
var filter = new Filter("IsActive", WhereCondition.Equal, "true");
// clients.Where(x => x.IsActive == true)
```

```json
{ "where": { "column": "isActive", "condition": "=", "value": "true" } }
```

## Regras da conversão

1. Se o tipo do `Value` já é o mesmo da propriedade, é usado como está.
2. Para `string`, o valor é convertido via `Convert.ToString`.
3. Para `enum`, usa `Enum.Parse` (nome ou valor numérico).
4. Para os demais tipos, usa `TryParse` da cultura invariante (números, datas, `Guid`, `bool`, ...).
5. Falha na conversão lança `InvalidCastException` com a mensagem `Cannot convert value to type {type}.`

## Referência

- [`WhereExpression.TryCastColumnValueType`](../src/Vip.DynamicFilter/Expressions/WhereExpression.cs)
