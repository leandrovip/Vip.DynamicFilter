# Referência da API pública

Assinaturas completas de tudo que é `public` no pacote. Namespace único: `Vip.DynamicFilter`.

## Modelos

### `Where`

```csharp
public class Where
{
    public Where();                                        // ConditionType = None

    public string Column { get; set; }
    public WhereCondition ConditionType { get; set; }
    public object Value { get; set; }
    public string Condition { get; set; }                  // setter converte o token e grava ConditionType

    public static Where New(string column, WhereCondition condition, object value);
}
```

### `Filter : Where`

```csharp
public class Filter : Where
{
    public Filter();                                       // Filters = new(), OperatorType = None
    public Filter(string column, WhereCondition conditionType, object value);

    public Operator OperatorType { get; set; }
    public List<Filter> Filters { get; set; }
    public string Operator { get; set; }                   // setter converte o token e grava OperatorType
}
```

### `Order`

```csharp
public class Order
{
    public Order();                                        // DirectionType = Asc
    public Order(string column, OrderDirection direction);

    public string Column { get; set; }
    public OrderDirection DirectionType { get; set; }
    public string Direction { get; set; }                  // setter converte o token e grava DirectionType
}
```

### `FilterRequest`

```csharp
public class FilterRequest
{
    public FilterRequest();                                // Limit = -1

    public Filter Where { get; set; }
    public List<Order> OrderBy { get; set; }
    public int PageNumber { get; set; }                    // default 0
    public int Limit { get; set; }                         // default -1
}
```

### `FilterResponse<T>`

```csharp
public class FilterResponse<T>
{
    public FilterResponse(IEnumerable<T> data, int totalCount, int pageNumber, int limit);
    public FilterResponse(IEnumerable<T> data);            // totalCount, pageNumber e limit = 0

    public IEnumerable<T> Data { get; }                    // setter privado
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int Limit { get; }
}
```

Não há `TotalPages`; calcule com `Math.Ceiling(TotalCount / (double) Limit)` quando `Limit > 0`. Como os setters são privados e não há construtor sem parâmetros, o tipo serve para **serializar** respostas; para deserializar com `System.Text.Json` é preciso que os nomes do JSON casem com os parâmetros do construtor de 4 argumentos.

## Enums

```csharp
public enum WhereCondition
{
    None, Equal, NotEqual, LessThan, GreaterThan, LessThanOrEqual, GreaterThanOrEqual,
    Contains, ContainsIgnoreCase, NotContains, StartsWith, NotStartsWith, EndsWith, NotEndsWith,
    Any, NotAny, IsNull, IsNotNull, IsEmpty, IsNotEmpty, IsNullOrEmpty, IsNotNullOrEmpty
}

public enum Operator { None, And, Or }

public enum OrderDirection { Asc, Desc }
```

São 22 membros em `WhereCondition` (21 condições utilizáveis + `None`). Tokens JSON de cada um em [filtros.md](filtros.md).

## Extensões — `QueryableExtensions`

Todos os métodos existem em duas sobrecargas: sobre `IQueryable<T>` e sobre `IEnumerable<T>` (esta chama `AsQueryable()` e delega). Todos retornam `IQueryable<T>` e têm execução adiada.

```csharp
public static IQueryable<T> ApplyFilterRequest<T>(this IQueryable<T>  query, FilterRequest request);
public static IQueryable<T> ApplyFilterRequest<T>(this IEnumerable<T> query, FilterRequest request);

public static IQueryable<T> Where<T>(this IQueryable<T>  query, Where filter);
public static IQueryable<T> Where<T>(this IEnumerable<T> query, Where filter);
public static IQueryable<T> Where<T>(this IQueryable<T>  query, Filter filter);
public static IQueryable<T> Where<T>(this IEnumerable<T> query, Filter filter);

public static IQueryable<T> OrderBy<T>(this IQueryable<T>  query, Order sort);
public static IQueryable<T> OrderBy<T>(this IEnumerable<T> query, Order order);
public static IQueryable<T> OrderBy<T>(this IQueryable<T>  query, IEnumerable<Order> orders);
public static IQueryable<T> OrderBy<T>(this IEnumerable<T> query, IEnumerable<Order> orders);

public static IQueryable<T> ThenBy<T>(this IQueryable<T>  query, Order order);
public static IQueryable<T> ThenBy<T>(this IEnumerable<T> query, Order order);
```

| Método | Argumento nulo | Observações |
|---|---|---|
| `ApplyFilterRequest` | `request == null` → tratado como `new FilterRequest()` (devolve a query intacta) | Ordem fixa: `Where` → `OrderBy` → `Skip` → `Take`. `Skip` só com `PageNumber > 0 && Limit > 0`; `Take` só com `Limit > 0` |
| `Where(Where)` | devolve a query | Trata o argumento como condição simples, mesmo que o objeto seja um `Filter` referenciado como `Where` |
| `Where(Filter)` | devolve a query | Resolve grupos `And`/`Or` recursivamente |
| `OrderBy(Order)` | devolve a query | Declarado como `IQueryable<T>`, mas o objeto retornado é `IOrderedQueryable<T>` |
| `OrderBy(IEnumerable<Order>)` | devolve a query | Itens nulos da lista são pulados; lista vazia devolve a query; primeiro item vira `OrderBy`, os demais `ThenBy` |
| `ThenBy(Order)` | devolve a query | Se a query não for `IOrderedQueryable<T>`, aplica `OrderBy` |

> As extensões `Where`/`OrderBy`/`ThenBy` têm o mesmo nome das do `System.Linq`. A resolução de sobrecarga escolhe pelo tipo do argumento (`Where`/`Filter`/`Order` da biblioteca × lambda), então as duas convivem no mesmo arquivo.

## Extensões — `ResponseExtensions`

```csharp
public static FilterResponse<T> GetFilterResponse<T>(this IQueryable<T>  query, FilterRequest request) where T : class;
public static FilterResponse<T> GetFilterResponse<T>(this IEnumerable<T> query, FilterRequest request) where T : class;
```

- **Síncrono** e de execução imediata: faz `Count()` e depois `ToList()` — duas idas à fonte de dados. Não existe versão `async`.
- `request == null` lança `NullReferenceException` (diferente de `ApplyFilterRequest`).
- `TotalCount` é contado após `Where`/`OrderBy` e antes de `Skip`/`Take`.
- `Skip` é aplicado com `PageNumber > 0` (sem exigir `Limit > 0`); `Take` com `Limit > 0`.
- `PageNumber` e `Limit` da resposta são um eco do request (com o default, `Limit` volta `-1`).

### Uso lado a lado

```csharp
var request = new FilterRequest
{
    Where = new Filter("Age", WhereCondition.GreaterThanOrEqual, 18),
    OrderBy = new List<Order> { new("Name", OrderDirection.Asc) },
    PageNumber = 1,
    Limit = 10
};

IQueryable<Client> query = clients.ApplyFilterRequest(request);       // adiado
FilterResponse<Client> page = clients.GetFilterResponse(request);     // executa agora
```

```json
{
  "where": { "column": "age", "condition": ">=", "value": 18 },
  "orderBy": [ { "column": "name", "direction": "asc" } ],
  "pageNumber": 1,
  "limit": 10
}
```

## Tipos internos (não fazem parte do contrato)

`WhereExpression`, `OrderExpression` e `OrderStep` são `internal`. Não há `InternalsVisibleTo`: os testes exercitam apenas a API pública. Detalhes em [arquitetura.md](arquitetura.md).

## Referência

- [`QueryableExtensions`](../src/Vip.DynamicFilter/Extensions/QueryableExtensions.cs)
- [`ResponseExtensions`](../src/Vip.DynamicFilter/Extensions/ResponseExtensions.cs)
- [`Models/`](../src/Vip.DynamicFilter/Models)
- [`Enums/`](../src/Vip.DynamicFilter/Enums)
