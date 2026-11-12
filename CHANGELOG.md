# Changelog

Todas as mudanças relevantes do pacote [`Vip.DynamicFilter`](https://www.nuget.org/packages/Vip.DynamicFilter/) são registradas aqui.

O formato segue o [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/). A versão é gerada pelo workflow de publicação como `1.0.<número da execução>`, portanto o último dígito **não** segue versionamento semântico: uma versão pode conter quebra de compatibilidade (indicada abaixo como **Quebra**).

> O repositório não tem tags. A correspondência entre versões e commits foi reconstruída cruzando as datas de publicação no NuGet com o histórico do Git.

## [Não publicado]

### Adicionado

- `Condition` aceita o **nome do enum** além do token: `"GreaterThanOrEqual"` equivale a `">="`.
- `Condition` passa a ignorar maiúsculas/minúsculas e espaços nas pontas (`"ISNULL"`, `" >= "`), como `Operator` e `Direction` já faziam.

### Alterado

- **Quebra:** `Operator` com valor desconhecido (ex.: `"xor"`) lança `ArgumentException` ao aplicar o filtro. Antes virava `None` em silêncio e o grupo era lido como condição simples.
- **Quebra:** `Direction` com valor desconhecido lança `ArgumentException` ao aplicar a ordenação. Antes virava `Asc` em silêncio.
- `Condition` com valor desconhecido continua lançando `ArgumentException`, agora com mensagem que informa o valor recebido e a coluna (`Condition 'x' is not recognized for column 'y'.`) em vez de `Filter type cannot be None for single filter.`

### Documentação

- README reescrito com exemplos C# e JSON de todas as funcionalidades.
- Pasta `docs/` com páginas de referência: filtros, operadores, ordenação, paginação, `FilterResponse<T>`, propriedades aninhadas, enums e tipos, serialização JSON, EF Core, limites e comportamentos.
- Novas páginas: arquitetura interna, referência completa da API, matriz de comportamentos verificados, segurança e performance, desenvolvimento (build, testes, CI) e DemoApi.
- `AGENTS.md` e `CLAUDE.md` com diretrizes para agentes de IA.
- Corrigido em `docs/enums-e-tipos.md`: a conversão de valores usa a cultura corrente da thread, não a invariante.
- Corrigido em `docs/ef-core.md`: exemplos usavam `await` em `GetFilterResponse`, que é síncrono.

### Demo e ferramentas

- DemoApi: modelos `Client` / `Address` / `ClientEnum`, seed de dados em memória e endpoints `/clients`, `/clients/{id}`, `/clients/query` e `/addresses`.
- Projetos de testes e DemoApi migrados de `net7.0` para `net10.0` (a biblioteca continua `netstandard2.0`).
- DemoApi: Swashbuckle.AspNetCore 6.5.0 → 10.3.1; removidos o pacote `Microsoft.AspNetCore.OpenApi` e as chamadas `WithOpenApi()`, descontinuadas no .NET 10.
- Workflow de publicação: SDK .NET 10, `actions/checkout@v4`, `actions/setup-dotnet@v4` e variáveis via `$GITHUB_ENV` no lugar do `::set-env` descontinuado.
- Coleção Postman ampliada de 7 para 28 requests, incluindo exemplos de `condition` por nome do enum e de condição inválida.

## [1.0.5] - 2026-03-16

### Alterado

- **Quebra:** `OrderBy` e `ThenBy` (todas as sobrecargas) passam a declarar retorno `IQueryable<T>` em vez de `IOrderedQueryable<T>`. Código que atribuía o resultado a `IOrderedQueryable<T>` precisa de ajuste, e assemblies compilados contra versões anteriores precisam ser recompilados.
- `ApplyFilterRequest` só aplica `Skip` quando `PageNumber > 0` **e** `Limit > 0` (antes bastava `PageNumber > 0`).
- `ThenBy` sobre uma query que não é `IOrderedQueryable<T>` passa a aplicar `OrderBy` em vez de falhar na conversão.

### Corrigido

- `ApplyFilterRequest` com `request` nulo não lança mais `NullReferenceException`: devolve a query sem alterações.
- `OrderBy` com `Order` nulo, lista nula ou lista vazia não lança mais `InvalidCastException` quando a query não é ordenada: devolve a query original.
- `OrderBy(IEnumerable<Order>)` ignora itens nulos da lista.

### Testes

- Novos testes para filtro por enum e para request com `Where` nulo.

## [1.0.4]

Não consta no NuGet. Provavelmente o número foi consumido por uma execução do workflow que não concluiu a publicação.

## [1.0.3] - 2023-05-11

### Adicionado

- Construtor `FilterResponse<T>(IEnumerable<T> data)`, que cria a resposta sem informações de paginação (`TotalCount`, `PageNumber` e `Limit` iguais a `0`).

## [1.0.2] - 2023-05-10

### Corrigido

- Condições de texto (`~`, `~~`, `!~`, `*~`, `!*~`, `~*`, `!~*`) falhavam quando o `FilterRequest` vinha de JSON, porque `Value` chega como `JsonElement` e não como `string`. O valor agora é convertido com `ToString()`.
- Comparações (`=`, `!=`, ...) em propriedades `string` com valor de outro tipo (número, `JsonElement`) passam a converter o valor para texto.

### Adicionado

- Coleção Postman em `collections/postman.json`.

## [1.0.1] - 2023-04-18

Primeira versão publicada no NuGet.

### Adicionado

- Filtros dinâmicos sobre `IQueryable<T>` e `IEnumerable<T>` com as 21 condições de `WhereCondition` e seus tokens JSON (`=`, `!=`, `<`, `>`, `<=`, `>=`, `~`, `~~`, `!~`, `*~`, `!*~`, `~*`, `!~*`, `any`, `!any`, `isnull`, `notnull`, `isempty`, `notempty`, `isnullorempty`, `notnullorempty`).
- Filtros compostos com `Operator.And` / `Operator.Or` e aninhamento (`Filter.Filters`).
- Navegação por propriedades aninhadas e coleções (`Address.Street` gera `Any(...)`).
- Conversão automática de `Value` para o tipo da propriedade (numéricos, datas, `Guid`, `bool`, enums, `string`).
- Ordenação simples e múltipla (`Order`, `OrderBy`, `ThenBy`).
- Paginação com `FilterRequest.PageNumber` / `Limit` e extensão `ApplyFilterRequest`.
- Resposta paginada `FilterResponse<T>` e extensão `GetFilterResponse`.
- Projeto de testes (xUnit), DemoApi (Minimal API) e workflow de publicação no NuGet.

[Não publicado]: https://github.com/leandrovip/Vip.DynamicFilter/commits/master
[1.0.5]: https://www.nuget.org/packages/Vip.DynamicFilter/1.0.5
[1.0.3]: https://www.nuget.org/packages/Vip.DynamicFilter/1.0.3
[1.0.2]: https://www.nuget.org/packages/Vip.DynamicFilter/1.0.2
[1.0.1]: https://www.nuget.org/packages/Vip.DynamicFilter/1.0.1
