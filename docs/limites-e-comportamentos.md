# Limites e comportamentos

Resumo dos comportamentos, defaults e casos de borda da biblioteca.

## Defaults

| Item | Default | Onde |
|---|---|---|
| `FilterRequest.Limit` | `-1` (sem limite) | `FilterRequest` ctor |
| `FilterRequest.PageNumber` | `0` | — |
| `Filter.OperatorType` | `Operator.None` | `Filter` ctor |
| `Order.DirectionType` | `OrderDirection.Asc` | `Order` ctor |
| `Where.ConditionType` | `WhereCondition.None` | `Where` ctor |

## Validações e exceções

| Situação | Exceção |
|---|---|
| `Column` vazia ou `ConditionType == None` em filtro simples | `ArgumentException` — "Filter type cannot be None for single filter." |
| Operador `And`/`Or` com `Filters` vazio | `ArgumentException` — "Filters with operator type different from Operator.None cannot be empty." |
| `Filter`/`Where`/`Order` nulos passados às extensões | Ignorados (sem erro), com exceção dos métodos internos |
| Propriedade não encontrada (case-insensitive) | `InvalidOperationException` — "Property '{name}' not found on type '{type}'" |
| Valor não conversível ao tipo da propriedade | `InvalidCastException` — "Cannot convert value to type {type}." |
| Condição de string (`isempty`, `notempty`, `isnullorempty`, `notnullorempty`) em propriedade não-string | `InvalidCastException` — "can be applied to String type only" |

## Comportamentos

- **Nome de propriedade case-insensitive**: `name` ≡ `Name`.
- **`Where` / `OrderBy` nulos** em `FilterRequest` são ignorados.
- **`OrderBy` nulo** ou lista vazia retorna a query sem ordenação.
- **`Contains` (`~`)** é case-sensitive; use `ContainsIgnoreCase` (`~~`) para ignorar caixa.
- **Conversão de tipos**: valores são convertidos ao tipo da propriedade (int, decimal, DateTime, Guid, bool, enum, string). Veja [enums-e-tipos.md](enums-e-tipos.md).
- **Navegação em coleções**: `Address.Street` gera `Address.Any(a => a.Street...)`. Veja [propriedades-aninhadas.md](propriedades-aninhadas.md).
- **Paginação 1-based**: `PageNumber = 1` é a primeira página.
- **`ApplyFilterRequest`**: `Skip` apenas com `PageNumber > 0 && Limit > 0`; `Take` apenas com `Limit > 0`.
- **`GetFilterResponse`**: conta o total **antes** do `Skip`/`Take`; exige `T : class`.

## Casos de borda conhecidos

- Com `GetFilterResponse`, se `PageNumber > 0` e `Limit <= 0` (default `-1`), o `Skip` recebe um valor negativo. Em LINQ to Objects isso é tratado como "sem skip", mas recomenda-se informar `PageNumber` e `Limit` juntos.
- A navegação em coleção é suportada para **um nível** abaixo da coleção (`Address.Street`). Níveis mais profundos em coleções aninhadas não são o cenário validado.
- Condições de comparação em `string` (`=`, `!=`) usam comparação ordinal (case-sensitive).
- `any` / `!any` exigem uma propriedade do tipo `IEnumerable<T>`.

## Compatibilidade

- .NET Standard 2.0.
- Funciona com `IQueryable<T>` (EF Core, LINQ to SQL) e `IEnumerable<T>` (coleções em memória).
- `netstandard2.0` usa C# `latest` (`LangVersion`).

## Referência

- [`WhereExpression`](../src/Vip.DynamicFilter/Expressions/WhereExpression.cs)
- [`Where`](../src/Vip.DynamicFilter/Models/Where.cs)
- [`Filter`](../src/Vip.DynamicFilter/Models/Filter.cs)
- [`Order`](../src/Vip.DynamicFilter/Models/Order.cs)
