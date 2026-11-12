# Segurança e performance

Cuidados ao expor `FilterRequest` diretamente em uma API pública. A biblioteca executa o que recebe: **não há** lista de colunas permitidas, limite máximo de página nem limite de profundidade.

## Segurança

### 1. Qualquer propriedade pública é filtrável e ordenável

O cliente escolhe `column` livremente, inclusive propriedades que você não devolve na resposta. Com `*~` (começa com) e respostas sim/não, dá para descobrir um valor caractere a caractere:

```csharp
// "Existe usuário cujo hash começa com 'a'?" — repetindo, extrai-se o valor inteiro
new Filter("PasswordHash", WhereCondition.StartsWith, "a")
```

```json
{ "where": { "column": "passwordHash", "condition": "*~", "value": "a" } }
```

A ordenação vaza informação do mesmo jeito (a posição relativa dos registros revela o valor).

**Mitigações**, em ordem de preferência:

- Aplique o filtro sobre uma **projeção/DTO** que só contém o que pode ser consultado: `_context.Users.Select(u => new UserDto { ... }).GetFilterResponse(request)`.
- Valide o request contra uma **lista de colunas permitidas** antes de aplicar.

Exemplo de validador (código de aplicação, não faz parte da biblioteca):

```csharp
public static class FilterGuard
{
    public static void Ensure(FilterRequest request, ISet<string> allowed, int maxLimit, int maxDepth = 3)
    {
        if (request == null) return;

        Check(request.Where, allowed, maxDepth);

        foreach (var order in request.OrderBy ?? new List<Order>())
            if (order == null || !allowed.Contains(order.Column ?? string.Empty))
                throw new ArgumentException("Coluna de ordenação não permitida.");

        if (request.Limit <= 0 || request.Limit > maxLimit) request.Limit = maxLimit;
    }

    private static void Check(Filter filter, ISet<string> allowed, int depth)
    {
        if (filter == null) return;
        if (depth < 0) throw new ArgumentException("Filtro com aninhamento excessivo.");

        if (filter.OperatorType == Operator.None)
        {
            if (!allowed.Contains(filter.Column ?? string.Empty))
                throw new ArgumentException("Coluna de filtro não permitida.");
            return;
        }

        foreach (var child in filter.Filters ?? new List<Filter>())
        {
            if (child == null) throw new ArgumentException("Filtro nulo.");
            Check(child, allowed, depth - 1);
        }
    }
}

// Uso — comparador case-insensitive, como a resolução de colunas da biblioteca
var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Name", "Age", "ClientType", "Address.Street" };
FilterGuard.Ensure(request, allowed, maxLimit: 100);
```

```json
{
  "where": { "column": "name", "condition": "~", "value": "Silva" },
  "orderBy": [ { "column": "age", "direction": "desc" } ],
  "pageNumber": 1,
  "limit": 500
}
```

No exemplo acima o `limit` 500 é reduzido para 100 pelo validador.

### 2. Sem limite de página

O default de `Limit` é `-1` e qualquer valor `<= 0` significa "sem `Take`". Um request vazio (`{}`) devolve a tabela inteira. Imponha um teto no servidor (como no validador acima).

### 3. Filtro ignorado por configuração de JSON

Se o deserializador não mapear camelCase, `Where` chega `null` e a consulta devolve **tudo** sem erro (ver [matriz-de-comportamentos.md](matriz-de-comportamentos.md#deserialização)). Em endpoints onde o filtro é obrigatório, valide `request.Where != null`. Nunca use o filtro do cliente como controle de acesso: restrições de tenant/usuário devem ser aplicadas no servidor **antes** de `ApplyFilterRequest`:

```csharp
var query = _context.Clients.Where(c => c.TenantId == tenantId);   // regra do servidor
return query.GetFilterResponse(request);                           // critérios do cliente
```

```json
{ "where": { "column": "name", "condition": "~", "value": "Silva" } }
```

### 4. Aninhamento e tamanho

`Filters` aceita profundidade e quantidade arbitrárias; a montagem é recursiva. Limite profundidade e número de condições (e o tamanho do corpo da requisição) para evitar consultas caras ou estouro de pilha.

### 5. Injeção de SQL

Os valores viram nós da árvore de expressões (constantes ou parâmetros de closure), nunca texto concatenado. A proteção contra injeção é a do provedor LINQ utilizado. Não há execução de SQL bruto nem de expressões em string na biblioteca.

### 6. Erros como resposta

Filtros inválidos lançam exceções genéricas do .NET, cujas mensagens incluem nomes de tipos e propriedades (`Property 'X' not found on type 'Namespace.Entidade'`). Converta em HTTP 400 com mensagem neutra em vez de devolver o detalhe — exemplo em [matriz-de-comportamentos.md](matriz-de-comportamentos.md#exceções-embrulhadas).

## Performance

- **Montagem da árvore**: feita por reflexão a cada chamada, sem cache de `PropertyInfo` nem de expressões. O custo é pequeno frente a uma ida ao banco, mas existe em laços apertados sobre coleções em memória (onde a expressão ainda é compilada a cada execução).
- **Parametrização**: valores de comparação (`=`, `<`, ...) são emitidos no formato de closure para que o provedor os trate como parâmetros; valores de `~`, `*~`, `~*` entram como constantes. Detalhes em [arquitetura.md](arquitetura.md#4-valores-como-parâmetro--tostaticparameterexpressionoftype).
- **`GetFilterResponse` faz duas consultas** (`Count()` + `ToList()`), ambas síncronas. Em alta concorrência com EF Core, prefira montar com `ApplyFilterRequest` e materializar com `ToListAsync()` / `CountAsync()`:

```csharp
var filtered = _context.Clients.Where(request.Where).OrderBy(request.OrderBy);
var total = await filtered.CountAsync();

if (request.PageNumber > 0 && request.Limit > 0)
    filtered = filtered.Skip((request.PageNumber - 1) * request.Limit);
if (request.Limit > 0)
    filtered = filtered.Take(request.Limit);

var response = new FilterResponse<Client>(await filtered.ToListAsync(), total, request.PageNumber, request.Limit);
```

```json
{
  "where": { "column": "age", "condition": ">=", "value": 18 },
  "orderBy": [ { "column": "name", "direction": "asc" } ],
  "pageNumber": 1,
  "limit": 20
}
```

- **Paginação sem ordenação**: `Skip`/`Take` sem `orderBy` não garante ordem estável em banco de dados. Exija ou aplique uma ordenação padrão.
- **Condições que impedem uso de índice**: `~` (contém) e `~*` (termina com) geram `LIKE '%...'`; `*~` (começa com) é a única da família que costuma usar índice.
- **Coleções**: `Address.Street` gera `Any(...)`, traduzido como subconsulta `EXISTS`.
