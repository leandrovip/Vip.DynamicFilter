# Deserialização de JSON

O `FilterRequest` pode ser recebido diretamente do corpo de uma requisição HTTP (JSON). Para que os **tokens** (`condition`, `operator`, `direction`) sejam interpretados corretamente, é preciso configurar a serialização.

## Por que a configuração é necessária?

As classes `Where`, `Filter` e `Order` possuem **propriedades string** (`Condition`, `Operator`, `Direction`) cujos *setters* convertem o token para o enum correspondente. Sem essa camada, o JSON precisa usar os **nomes dos enums** (`"equal"`, `"and"`, `"desc"`), o que é menos legível.

| Propriedade string | Tokens aceitos | Enum resultante |
|---|---|---|
| `Condition` | `"="`, `"!="`, `"<"`, `">"`, `"<="`, `">="`, `"~"`, `"~~"`, `"!~"`, `"*~"`, `"!*~"`, `"~*"`, `"!~*"`, `"any"`, `"!any"`, `"isnull"`, `"notnull"`, `"isempty"`, `"notempty"`, `"isnullorempty"`, `"notnullorempty"` | `WhereCondition` |
| `Operator` | `"and"`, `"or"`, `"none"` | `Operator` |
| `Direction` | `"asc"`, `"desc"` | `OrderDirection` |

## System.Text.Json

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

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

- `PropertyNamingPolicy.CamelCase`: faz o binding de `pageNumber`, `orderBy`, `column`, `condition`, etc.
- `JsonStringEnumConverter`: permite usar os nomes dos enums (`"equal"`, `"desc"`) como alternativa aos tokens, e serializar as respostas (`FilterResponse`).

### Sem converter (apenas tokens)

Se o JSON usar **somente tokens**, o `JsonStringEnumConverter` não é obrigatório — as propriedades string já convertem. O `PropertyNamingPolicy.CamelCase` continua recomendado:

```csharp
var request = JsonSerializer.Deserialize<FilterRequest>(jsonBody, new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
});
```

## Newtonsoft.Json

```csharp
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

var settings = new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver()
};

var request = JsonConvert.DeserializeObject<FilterRequest>(jsonBody, settings);
```

## Exemplos de payload

### Filtro simples

```json
{
  "where": {
    "column": "name",
    "condition": "~~",
    "value": "batman"
  }
}
```

### Filtro composto + ordenação + paginação

```json
{
  "where": {
    "operator": "and",
    "filters": [
      { "column": "name", "condition": "~~", "value": "silva" },
      {
        "operator": "or",
        "filters": [
          { "column": "age", "condition": "=", "value": 22 },
          { "column": "age", "condition": "=", "value": 25 }
        ]
      }
    ]
  },
  "orderBy": [
    { "column": "age", "direction": "desc" },
    { "column": "name", "direction": "asc" }
  ],
  "pageNumber": 1,
  "limit": 10
}
```

### Apenas ordenação

```json
{
  "orderBy": [
    { "column": "age", "direction": "desc" }
  ]
}
```

### Apenas paginação

```json
{
  "pageNumber": 1,
  "limit": 10
}
```

### Requisição vazia (retorna todos os registros)

```json
{}
```

## Atalho: deserializar para aplicar

```csharp
// Exemplo em um endpoint
app.MapPost("/clients", (FilterRequest filter) => clients.GetFilterResponse(filter));

// Cliente HTTP (JSON)
var body = JsonSerializer.Serialize(request, JsonOptions.Default);
```

## Observações

- Os **nomes das propriedades no JSON** usam camelCase: `pageNumber`, `limit`, `where`, `orderBy`, `column`, `condition`, `operator`, `filters`, `value`, `direction`.
- Valores podem ser numéricos ou strings (`"value": 22` ou `"value": "22"`), ambos convertidos ao tipo da propriedade. Veja [enums-e-tipos.md](enums-e-tipos.md).
- Existe uma coleção [Postman](../collections/postman.json) no repositório com exemplos prontos para a `DemoApi`.

## Referência

- [`MapperTests`](../tests/Vip.DynamicFilter.Tests/MapperTests.cs)
- [filtros.md](filtros.md) — tabela completa de condições
