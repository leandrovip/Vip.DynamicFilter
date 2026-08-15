# AGENTS.md

Diretrizes para agentes de IA trabalharem neste repositório.

## Visão geral

`Vip.DynamicFilter` é uma biblioteca .NET Standard 2.0 que gera **filtros dinâmicos** usando expression trees. Recebe critérios via objetos C# ou JSON e os aplica em `IQueryable<T>` / `IEnumerable<T>` (filtro, ordenação e paginação).

## Estrutura do repositório

```
src/
  Vip.DynamicFilter.sln          # Solução principal
  Vip.DynamicFilter/             # Biblioteca (netstandard2.0)
    Models/                      # Where, Filter, Order
    Requests/                    # FilterRequest
    Responses/                   # FilterResponse<T>
    Enums/                       # Operator, WhereCondition, OrderDirection, OrderStep (interno)
    Expressions/                 # WhereExpression, OrderExpression (internos)
    Extensions/                  # QueryableExtensions, ResponseExtensions
tests/
  Vip.DynamicFilter.Tests/       # xUnit + Bogus (net7.0)
demo/
  Vip.DynamicFilter.DemoApi/     # Minimal API de exemplo (net7.0)
collections/
  postman.json                   # Coleção Postman da DemoApi
docs/                            # Documentação de referência (PT-BR)
README.md
AGENTS.md
```

## Comandos

```bash
# Build da solução
dotnet build ./src/Vip.DynamicFilter.sln

# Testes (xUnit)
dotnet test ./src/Vip.DynamicFilter.sln

# Pack do pacote NuGet
dotnet pack ./src/Vip.DynamicFilter/Vip.DynamicFilter.csproj -c Release
```

> A solução contém 3 projetos: biblioteca (`src`), testes (`tests`) e demo (`demo`).

## Convenções de código

- **C#**: usa regiões `#region` para organizar (Constructors, Properties, Methods). Siga esse padrão em arquivos existentes.
- **Idioma**: mensagens de commit, comentários e documentação em **português (PT-BR)**.
- **Pacote**: `Vip.DynamicFilter` no NuGet; versão definida no workflow [`nuget.yml`](.github/workflows/nuget.yml) como `1.0.${GITHUB_RUN_NUMBER}`.
- **Enums com tokens string**: `WhereCondition`, `Operator` e `OrderDirection` possuem tokens JSON (`"="`, `"~~"`, `"and"`, `"desc"`, ...) convertidos por propriedades string (`Condition`, `Operator`, `Direction`). Qualquer nova condição **deve** manter token + enum + conversão sincronizados.

## Convenções de documentação

- Todo exemplo deve mostrar **C# e JSON lado a lado**.
- Usar o **modelo de exemplo** `Client` / `Address` / `ClientEnum` (mesmo dos testes) para manter consistência.
- JSON usa **camelCase** (`pageNumber`, `orderBy`, `condition`, ...) e os **tokens** de condição.
- Alterações na API pública ou em condições exigem atualizar o `README.md` e a página correspondente em `docs/` (ver índice em `docs/README.md`).
- Ao adicionar/remover condições, atualizar a tabela em `README.md` e `docs/filtros.md`.

## Trabalho com testes

- Exemplos de documentação devem espelhar os testes unitários (`WhereTests`, `OrderTests`, `PagingTests`, `MapperTests`) para garantir fidelidade.
- Sempre rodar `dotnet test` após alterações no código-fonte.

## Notas

- `.gitignore` contém marcadores de conflito de merge não resolvidos (conteúdo duplicado entre `<<<<<<< HEAD` / `>>>>>>>`). Evite alterá-lo sem necessidade.
- O teste de serialização (`MapperTests`) usa `System.Text.Json` com `JsonStringEnumConverter` + `PropertyNamingPolicy.CamelCase` — referência para exemplos de deserialização JSON.
