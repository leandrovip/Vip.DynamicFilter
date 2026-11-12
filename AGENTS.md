# AGENTS.md

Diretrizes para agentes de IA trabalharem neste repositório.

## Visão geral

`Vip.DynamicFilter` é uma biblioteca .NET Standard 2.0, **sem dependências externas**, que gera **filtros dinâmicos** com expression trees. Recebe critérios via objetos C# ou JSON e os aplica em `IQueryable<T>` / `IEnumerable<T>` (filtro, ordenação e paginação). É publicada no NuGet como `Vip.DynamicFilter`.

## Estrutura do repositório

```
src/
  Vip.DynamicFilter.sln          # Solução (referencia também ../tests e ../demo)
  Vip.DynamicFilter/             # Biblioteca (netstandard2.0, LangVersion=latest)
    Models/                      # Where, Filter (: Where), Order
    Requests/                    # FilterRequest
    Responses/                   # FilterResponse<T>
    Enums/                       # WhereCondition, Operator, OrderDirection, OrderStep (interno)
    Expressions/                 # WhereExpression, OrderExpression (internos — o motor)
    Extensions/                  # QueryableExtensions, ResponseExtensions (fachada pública)
tests/
  Vip.DynamicFilter.Tests/       # xUnit + Bogus (net10.0) — 40 casos de teste
demo/
  Vip.DynamicFilter.DemoApi/     # Minimal API de exemplo (net10.0, Swagger)
collections/
  postman.json                   # Coleção Postman da DemoApi (28 requests)
docs/                            # Documentação de referência (PT-BR); índice em docs/README.md
.github/workflows/nuget.yml      # Publicação manual no NuGet (não há CI em push/PR)
```

Todo o código da biblioteca está no namespace único `Vip.DynamicFilter`; as pastas são apenas organização.

## Comandos

```bash
dotnet build ./src/Vip.DynamicFilter.sln
dotnet test  ./src/Vip.DynamicFilter.sln
dotnet test  ./src/Vip.DynamicFilter.sln --filter "FullyQualifiedName~WhereTests"
dotnet run   --project ./demo/Vip.DynamicFilter.DemoApi --launch-profile https   # https://localhost:7165/swagger
dotnet pack  ./src/Vip.DynamicFilter/Vip.DynamicFilter.csproj -c Release
```

Testes e demo têm como alvo `net10.0` e exigem o SDK .NET 10; a biblioteca continua `netstandard2.0`. O identificador correto é `net10.0` — `net10` é lido como .NET Framework 1.0 e quebra o build.
## Arquitetura em um parágrafo

`FilterRequest` → `QueryableExtensions.ApplyFilterRequest` aplica, nesta ordem, `Where` → `OrderBy` → `Skip` → `Take`. `WhereExpression` resolve grupos `And`/`Or` recursivamente, navega `Column` segmento a segmento (case-insensitive; ao encontrar uma coleção gera `Any(...)` e para), converte `Value` para o tipo da propriedade e monta a expressão pelo `switch` de `WhereCondition`. `OrderExpression` invoca `OrderBy`/`ThenBy` por reflexão. Detalhes em [`docs/arquitetura.md`](docs/arquitetura.md).

## Regras ao alterar o código

- **Condições têm três pontos de sincronização.** Adicionar/alterar uma condição exige mexer em: `Enums/WhereCondition.cs` (membro + `[Description]`), `Models/Where.cs` (dicionário em `CreateConditions`, token → enum) e `Expressions/WhereExpression.cs` (`GenerateExpressionByCondition`). O `[Description]` é decorativo; quem vale é o dicionário. O mesmo vale para `Operator` (`Filter.TryConvertOperator`) e `OrderDirection` (`Order.TryConvertDirection`).
- **Tokens são case-insensitive e com `Trim`**; `Condition` aceita também o nome do enum. Valor desconhecido **não lança no setter**: marca uma flag `internal` (`IsConditionUnknown`, `IsOperatorUnknown`, `IsDirectionUnknown`) e o motor lança `ArgumentException` ao aplicar. Mantenha esse padrão — lançar no setter embrulharia o erro em `JsonException`.
- **Compatibilidade `netstandard2.0`**: não use APIs que só existem em .NET moderno. Recursos de sintaxe (switch expression, `new()`, file-scoped namespace) funcionam por causa de `LangVersion=latest`, mas `init`, `record` e `required` não funcionam sem polyfills.
- **Sem dependências**: não adicione pacotes NuGet à biblioteca sem pedido explícito.
- **API pública é contrato**: `WhereExpression`, `OrderExpression` e `OrderStep` são `internal` e não há `InternalsVisibleTo` — teste pela API pública.
- **Execução adiada**: extensões de `QueryableExtensions` devolvem `IQueryable<T>` sem materializar. Só `GetFilterResponse` executa (`Count()` + `ToList()`).
- **Estilo**: `#region` para organizar (`Constructors`, `Properties`, `Methods`, ...); `Nullable` desabilitado; siga o estilo de namespace do arquivo que estiver editando (há arquivos com bloco e com *file-scoped*).
- **Não altere** `.github/workflows/nuget.yml` nem a versão do pacote sem pedido explícito: a versão é `1.0.${GITHUB_RUN_NUMBER}` e a publicação é manual.

## Comportamentos que surpreendem

Verificados em execução; a lista completa está em [`docs/matriz-de-comportamentos.md`](docs/matriz-de-comportamentos.md). Não "corrija" nenhum deles como efeito colateral de outra tarefa — são mudanças de comportamento.

- Conversão de `Value` usa a **cultura corrente** (`"10.5"` → `105` em pt-BR).
- `byte`/`sbyte` não anuláveis e enums anuláveis lançam `InvalidCastException`.
- `Value == null` em comparações lança `InvalidCastException` (usa-se `isnull`/`notnull`).
- Após uma coleção, só **um** segmento é considerado (`Address.Street`); o restante é descartado em silêncio.
- Erros dentro de um caminho de coleção chegam embrulhados em `TargetInvocationException`.
- `GetFilterResponse(null)` lança `NullReferenceException`; `ApplyFilterRequest(null)` devolve a query intacta.
- `Limit <= 0` (default `-1`) significa sem `Take`.
- Em memória não há *null guards*: `~` sobre propriedade `null` lança `NullReferenceException`.

## Testes

- Padrão: montar o filtro dinâmico, escrever o **LINQ equivalente à mão** e comparar com `Helper.EnumarableAreEqual` (o nome tem esse erro de grafia; não renomeie sem pedido).
- Dados: `Mock.Clients` (14 clientes fixos). Cidade e CEP dos endereços são aleatórios (Bogus) — não faça asserções sobre eles.
- Nomes: `<Classe>_<Método>_<Cenário>_<Resultado>`; corpo com `// Arrange / Act / Assert`.
- Sempre rode `dotnet test` após alterar código-fonte e relate o resultado real.
- `MapperTests` é a referência de deserialização: `System.Text.Json` com `JsonStringEnumConverter` + `PropertyNamingPolicy.CamelCase`.

## Convenções de documentação

- **Idioma**: commits, comentários e documentação em **português (PT-BR)**. Histórico recente usa prefixos no estilo Conventional Commits (`fix(filter):`, `docs:`).
- Todo exemplo de uso mostra **C# e JSON lado a lado**.
- Usar o **modelo de exemplo** `Client` / `Address` / `ClientEnum` (o mesmo dos testes).
- JSON em **camelCase** (`pageNumber`, `orderBy`, `condition`, ...) e com os **tokens** de condição.
- Exemplos devem espelhar os testes (`WhereTests`, `OrderTests`, `PagingTests`, `MapperTests`); afirmações sobre comportamento não coberto por teste devem ser verificadas em execução antes de serem escritas.
- Alterações na API pública ou em condições exigem atualizar o `README.md`, a página correspondente em `docs/` e [`docs/referencia-api.md`](docs/referencia-api.md).
- Ao adicionar/remover condições: tabelas de `README.md` e `docs/filtros.md`, a matriz em `docs/matriz-de-comportamentos.md` e um request em `collections/postman.json`.
- Toda mudança de comportamento, API ou correção entra na seção "Não publicado" do `CHANGELOG.md` (formato Keep a Changelog; quebras de compatibilidade marcadas com **Quebra**).
- Página nova em `docs/` entra no índice de `docs/README.md` e na seção "Documentação" do `README.md`.

## Mapa da documentação

| Preciso de... | Arquivo |
|---|---|
| Condições e tokens | `docs/filtros.md` |
| AND/OR e aninhamento | `docs/operadores.md` |
| Ordenação / paginação / resposta | `docs/ordenacao.md`, `docs/paginacao.md`, `docs/filter-response.md` |
| Coleções e propriedades aninhadas | `docs/propriedades-aninhadas.md` |
| Conversão de tipos | `docs/enums-e-tipos.md` |
| JSON | `docs/serializacao-json.md` |
| EF Core | `docs/ef-core.md` |
| Exceções e casos de borda | `docs/limites-e-comportamentos.md`, `docs/matriz-de-comportamentos.md` |
| Assinaturas da API | `docs/referencia-api.md` |
| Funcionamento interno | `docs/arquitetura.md` |
| Build, testes, CI, pacote | `docs/desenvolvimento.md` |
| DemoApi e Postman | `docs/demo-api.md` |
| Exposição em API pública | `docs/seguranca-e-performance.md` |

## Notas

- `.gitignore` contém marcadores de conflito de merge não resolvidos (`<<<<<<< HEAD` / `=======` / `>>>>>>>`). Evite alterá-lo sem necessidade.
- Ramo principal: `master`.
- Os ids da DemoApi (`Guid.NewGuid()`) mudam a cada inicialização.
