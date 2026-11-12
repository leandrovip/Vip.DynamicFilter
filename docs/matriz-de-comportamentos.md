# Matriz de comportamentos verificados

Complemento de [limites-e-comportamentos.md](limites-e-comportamentos.md). Tudo nesta página foi **executado** contra a biblioteca (coleções em memória, runtime .NET 10) — salvo onde indicado como "não verificado".

Legenda: ✅ funciona · ❌ lança exceção · ⚠️ funciona com ressalva.

## Condição × tipo da propriedade

| Condição | `string` | numéricos / `DateTime` / `TimeSpan` | `Guid` | `bool` | enum | coleção `ICollection<T>` / `List<T>` |
|---|---|---|---|---|---|---|
| `=` `!=` | ✅ ordinal, case-sensitive | ✅ | ✅ | ✅ | ✅ | ❌ |
| `<` `>` `<=` `>=` | ❌ `InvalidOperationException` (operador não definido) | ✅ | ⚠️ depende do runtime | ❌ `InvalidOperationException` | ❌ `InvalidOperationException` | ❌ |
| `~` `!~` `*~` `!*~` `~*` `!~*` `~~` | ✅ | ❌ `ArgumentException` | ❌ | ❌ | ❌ | ❌ |
| `isnull` `notnull` | ✅ | ❌ `NullReferenceException` na execução se não anulável; ✅ em `T?` | idem | idem | idem | ✅ |
| `isempty` `notempty` `isnullorempty` `notnullorempty` | ✅ | ❌ `InvalidCastException` | ❌ | ❌ | ❌ | ❌ |
| `any` `!any` | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |

Observações:

- As condições de string **não** lançam `InvalidCastException` em propriedade não-string (apenas a família `isempty`); o erro é `ArgumentException` vindo de `Expression.Call`.
- `any`/`!any` em propriedade que não é coleção genérica — inclusive `string` e **arrays** (`T[]`) — lança `InvalidOperationException: Sequence contains no elements`.

## Conversão de `Value`

| Cenário | Resultado |
|---|---|
| `int` ← `"14"`, `14`, `10.0` (double) | ✅ |
| `int?` ← `5` ou `"5"` | ✅ |
| `Guid` ← string | ✅ |
| `string` ← `10` (int) | ✅ (compara com `"10"`) |
| enum ← `"Servico"` ou `"2"` | ✅ |
| enum ← `"servico"` (caixa errada) ou nome inexistente | ❌ **`ArgumentException`** (`Requested value ... was not found`), não `InvalidCastException` |
| enum anulável (`ClientEnum?`) | ❌ `InvalidCastException` (`Cannot convert value to type Nullable`1`) |
| `byte` / `sbyte` **não anuláveis** | ❌ `InvalidCastException` mesmo com valor do tipo correto (ausentes da lista de tipos; `byte?`/`sbyte?` funcionam) |
| qualquer comparação com `Value == null` | ❌ `InvalidCastException` — use `isnull` / `notnull` |
| valor não conversível (`int` ← `"abc"`) | ❌ `InvalidCastException` |
| `Value` como array JSON | ❌ `InvalidCastException` (não existe condição `in`) |
| `~`, `*~`, `~*` e negações com `Value == null` | ❌ `NullReferenceException` |

### Cultura

A conversão usa `T.TryParse(string, out T)`, que respeita a **cultura corrente da thread**. O mesmo valor produz resultados diferentes conforme a cultura do servidor:

| Valor recebido | Tipo | Cultura invariante / en-US | Cultura pt-BR |
|---|---|---|---|
| `"10.5"` ou JSON `10.5` | `decimal` | 10,5 | **105** |
| `"10,5"` | `decimal` | 105 | 10,5 |
| `"01/02/2020"` | `DateTime` | 2 de janeiro | 1º de fevereiro |
| `"2020-02-01"` (ISO 8601) | `DateTime` | 1º de fevereiro | 1º de fevereiro |

Vale também para números JSON: `{"value": 10.5}` chega como `JsonElement`, vira o texto `"10.5"` e é reinterpretado na cultura corrente.

```csharp
// pt-BR: filtra Price == 105, não 10.5
new Filter("Price", WhereCondition.Equal, "10.5")
```

```json
{ "where": { "column": "price", "condition": "=", "value": 10.5 } }
```

Recomendações: datas sempre em ISO 8601; em APIs que recebem decimais, fixe a cultura da requisição (`UseRequestLocalization` com cultura invariante, ou `CultureInfo.DefaultThreadCurrentCulture`) ou converta o valor para o tipo exato antes de chamar a biblioteca.

## Navegação

| `Column` | Resultado |
|---|---|
| `Address.Street` (coleção → propriedade) | ✅ `Address.Any(a => a.Street ...)` |
| `Colecao.Objeto.Propriedade` | ⚠️ o segmento após `Objeto` é **descartado em silêncio**: a condição é aplicada a `Objeto`. Com `=` falha (`InvalidCastException`); com `isnull` devolve resultado sobre `Objeto == null` sem avisar |
| `Name.Length` (propriedade de `string`) | ❌ em filtro (`InvalidOperationException`, pois `string` é tratada como coleção); ✅ em ordenação |
| `Colecao.Propriedade` em **ordenação** | ❌ `InvalidOperationException: Property ... not found` |
| Coluna inexistente | ❌ `InvalidOperationException: Property 'X' not found on type 'T'` |

## Tokens e estrutura do filtro

| Cenário | Resultado |
|---|---|
| `condition` com caixa diferente ou espaços (`"ISNULL"`, `" >= "`) | ✅ normalizado |
| `condition` com o **nome do enum** (`"GreaterThanOrEqual"`, em qualquer caixa) | ✅ |
| `condition` desconhecida (`"maiorque"`, `"=>"`, `"3"`) | ❌ `ArgumentException: Condition 'x' is not recognized for column 'y'.` |
| `condition` ausente ou `"none"` em filtro simples | ❌ `ArgumentException: Filter type cannot be None for single filter.` |
| `operator` / `direction` com espaços ou maiúsculas (`" AND "`, `" DESC "`) | ✅ normalizados |
| `operator` desconhecido (`"xor"`) | ❌ `ArgumentException: Operator 'xor' is not recognized.` |
| `direction` desconhecida | ❌ `ArgumentException: Direction 'x' is not recognized for column 'y'.` |
| Token inválido seguido de atribuição explícita do enum (`ConditionType`, `OperatorType`, `DirectionType`) | ✅ vale o enum |
| `OperatorType == None` com `Filters` preenchido e sem `Column` | ❌ `ArgumentException` (os `Filters` são ignorados) |
| `OperatorType == And/Or` com `Column`/`Value` no próprio grupo | ✅ `Column`/`Value` do grupo são ignorados |
| `OperatorType == And/Or` com `Filters` vazio | ❌ `ArgumentException` |
| `conditionType` com o **nome do enum** no JSON (`"GreaterThan"`) | ✅ (exige `JsonStringEnumConverter`) |

## Ordenação e paginação

| Cenário | Resultado |
|---|---|
| `Order` com `Column == null` | ❌ `NullReferenceException` |
| `ThenBy(order)` sobre coleção em memória **não ordenada** | ❌ `IndexOutOfRangeException` na enumeração (`AsQueryable()` de lista já é `IOrderedQueryable<T>`, então o atalho para `OrderBy` não é tomado) |
| `OrderBy(order).ThenBy(order)` | ✅ |
| `ApplyFilterRequest(null)` | ✅ devolve tudo |
| `GetFilterResponse(null)` | ❌ `NullReferenceException` |
| `Limit = 0` ou `-1` | sem `Take`: devolve **todos** os registros |
| `GetFilterResponse` com `PageNumber = 3` e `Limit` default | devolve tudo; resposta ecoa `pageNumber: 3`, `limit: -1` |
| `PageNumber = 0` com `Limit = 2` | sem `Skip`, `Take(2)` — equivale à página 1 |

## LINQ to Objects × provedores SQL

- **Nulos em memória**: não há *null guards*. `~`, `*~`, `~*`, `~~` sobre um item cuja propriedade é `null` lançam `NullReferenceException` na enumeração. Em SQL isso não ocorre. Para coleções em memória, combine com `notnull` em um grupo `and` (o `AndAlso` faz curto-circuito):

```csharp
new Filter
{
    OperatorType = Operator.And,
    Filters = new List<Filter>
    {
        new("Name", WhereCondition.IsNotNull, null),
        new("Name", WhereCondition.Contains, "Silva")
    }
}
```

```json
{
  "where": {
    "operator": "and",
    "filters": [
      { "column": "name", "condition": "notnull" },
      { "column": "name", "condition": "~", "value": "Silva" }
    ]
  }
}
```

- **`~~` (`ContainsIgnoreCase`)**: usa `string.Contains(string, StringComparison)`, sobrecarga que só existe a partir do .NET Core 2.1 / .NET 5. Em .NET Framework 4.x a expectativa é falha ao montar a expressão, e provedores SQL em geral não traduzem essa sobrecarga (*não verificado em execução*).

## Exceções embrulhadas

Erros que acontecem **dentro de um caminho de coleção** (ex.: `Address.Inexistente`) chegam embrulhados em `TargetInvocationException`, porque o sub-lambda é gerado por reflexão. Ao tratar erros, inspecione `InnerException`:

```csharp
try
{
    return Results.Ok(clients.GetFilterResponse(request));
}
catch (Exception ex) when ((ex is TargetInvocationException ? ex.InnerException : ex)
                           is ArgumentException or InvalidCastException or InvalidOperationException)
{
    return Results.BadRequest("Filtro inválido.");
}
```

```json
{ "where": { "column": "address.inexistente", "condition": "=", "value": "x" } }
```

A biblioteca não define tipos de exceção próprios; sem tratamento, um filtro inválido vindo do cliente resulta em HTTP 500.

## Deserialização

| Cenário | Resultado |
|---|---|
| `System.Text.Json` **sem** opções (`JsonSerializer.Deserialize<FilterRequest>(json)`) com JSON camelCase | ⚠️ `Where` fica `null` e o filtro é **ignorado em silêncio** (devolve tudo) — os nomes são case-sensitive por padrão |
| Com `PropertyNamingPolicy = CamelCase` (ou os defaults web do ASP.NET Core) | ✅ |
| Serializar um `FilterRequest` montado em C# | `condition`, `operator` e `direction` saem `null`; `conditionType`, `operatorType` e `directionType` saem preenchidos |

Detalhes de configuração em [serializacao-json.md](serializacao-json.md).
