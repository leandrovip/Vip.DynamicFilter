# Desenvolvimento, testes e publicação

Guia para quem trabalha no repositório: ambiente, build, testes, CI e empacotamento.

## Projetos

A solução fica em `src/Vip.DynamicFilter.sln` e referencia três projetos (dois deles fora de `src/`):

| Projeto | Caminho | SDK / TFM | Dependências |
|---|---|---|---|
| `Vip.DynamicFilter` | `src/Vip.DynamicFilter/` | `Microsoft.NET.Sdk` / `netstandard2.0`, `LangVersion=latest` | nenhuma |
| `Vip.DynamicFilter.Tests` | `tests/Vip.DynamicFilter.Tests/` | `Microsoft.NET.Sdk` / `net10.0` | xunit 2.4.2, xunit.runner.visualstudio 2.4.5, Microsoft.NET.Test.Sdk 17.5.0, Bogus 34.0.2, coverlet.collector 3.2.0 |
| `Vip.DynamicFilter.DemoApi` | `demo/Vip.DynamicFilter.DemoApi/` | `Microsoft.NET.Sdk.Web` / `net10.0` | Swashbuckle.AspNetCore 10.3.1 |

Testes e demo usam `ProjectReference` para a biblioteca (não o pacote NuGet). `Nullable` está desabilitado nos três.

## Ambiente

- **.NET SDK 10** para compilar a solução e executar testes e demo (ambos têm como alvo `net10.0`).
- A biblioteca em si continua `netstandard2.0`; a mudança de alvo dos testes e da demo não afeta o pacote publicado.
- O identificador do framework é `net10.0`. Escrever `net10` é interpretado pelo MSBuild como .NET Framework 1.0 e falha com `MSB3644` / `NU1201`.

## Comandos

```bash
dotnet build ./src/Vip.DynamicFilter.sln                                   # build dos 3 projetos
dotnet test  ./src/Vip.DynamicFilter.sln                                   # 40 casos de teste xUnit
dotnet test  ./src/Vip.DynamicFilter.sln --filter "FullyQualifiedName~WhereTests"   # uma classe
dotnet test  ./src/Vip.DynamicFilter.sln --collect:"XPlat Code Coverage"   # cobertura (coverlet)
dotnet run   --project ./demo/Vip.DynamicFilter.DemoApi                    # sobe a API de demonstração
dotnet pack  ./src/Vip.DynamicFilter/Vip.DynamicFilter.csproj -c Release   # gera o .nupkg
```

## Testes

Estrutura de `tests/Vip.DynamicFilter.Tests/`:

| Arquivo | Conteúdo |
|---|---|
| `WhereTests.cs` (10) | Filtro simples, grupo `And`, coleção aninhada (`Address.Street`), grupo aninhado `And`/`Or`, `ContainsIgnoreCase`, enum por nome, `Where` nulo, condição e operador desconhecidos |
| `OrderTests.cs` (10) | `OrderBy` simples e múltiplo; ordenação via `ApplyFilterRequest` com e sem filtro; conversão e rejeição de `direction` |
| `PagingTests.cs` (3) | `Skip`/`Take`, request sem paginação, paginação sem ordenação |
| `MapperTests.cs` (17) | Deserialização `System.Text.Json`; `condition` por token ou nome do enum; valores desconhecidos |
| `Core/Mock.cs` | `Mock.Clients`: `IQueryable<Client>` estático com 14 clientes fixos |
| `Core/Helper.cs` | `Helper.EnumarableAreEqual` (sic): compara duas sequências item a item, na ordem |
| `Models/` | `Client`, `Address`, `ClientEnum` |

Padrão de todos os testes: montar o filtro dinâmico, montar o **LINQ equivalente escrito à mão** e comparar as duas sequências com o helper. Nomes seguem `<Classe>_<Método>_<Cenário>_<Resultado>` e o corpo usa `// Arrange / Act / Assert`.

Pontos de atenção:

- Nomes, idades e ruas do mock são fixos; **cidade e CEP são aleatórios** (Bogus, `pt_BR`) — não escreva asserções sobre eles.
- `Birthday` é `DateTime.Now.AddDays(-idade)` nos testes e `AddYears(-idade)` na demo.
- O mock não tem valores nulos nem coleções vazias; a demo tem um cliente "Sem Endereço".

Lacunas de cobertura (cenários sem teste automatizado): `GetFilterResponse`/`FilterResponse<T>`, `ThenBy`, `Where(Where)`, as condições `<`, `>`, `!~`, `*~`, `!*~`, `~*`, `!~*`, `any`, `!any` e toda a família `isnull`/`isempty`, conversões de `Guid`/`DateTime`/`bool`, e os caminhos de exceção de conversão de valor e de navegação. O comportamento atual desses cenários está registrado em [matriz-de-comportamentos.md](matriz-de-comportamentos.md).

## Como adicionar uma condição

1. `Enums/WhereCondition.cs` — novo membro com `[Description("token")]`.
2. `Models/Where.cs` — nova entrada no dicionário de `CreateConditions` (token → enum).
3. `Expressions/WhereExpression.cs` — novo `case` em `GenerateExpressionByCondition`.
4. Teste em `WhereTests.cs` comparando com o LINQ equivalente.
5. Tabelas de condições em `README.md` e `docs/filtros.md`; request de exemplo em `collections/postman.json`.

Se o passo 2 for esquecido, o token não é reconhecido (só o nome do enum funciona) e o filtro lança `ArgumentException`. Se o passo 3 for esquecido, o `default` do `switch` devolve a própria propriedade e a falha aparece ao montar o lambda.

## CI e publicação NuGet

Há um único workflow, [`.github/workflows/nuget.yml`](../.github/workflows/nuget.yml) ("Publish NuGet.org"):

| Item | Valor |
|---|---|
| Gatilho | **Somente manual** (`workflow_dispatch`). Não há CI em push nem em pull request |
| Entrada | `typeBuild` (opcional): sufixo de pré-lançamento, ex. `-beta` |
| Runner | `ubuntu-latest`, .NET `10.0.x` (`actions/checkout@v4`, `actions/setup-dotnet@v4`) |
| Versão do assembly | `1.0.${GITHUB_RUN_NUMBER}` |
| Versão do pacote | `1.0.${GITHUB_RUN_NUMBER}${typeBuild}` (ex.: `1.0.42-beta`) |
| Passos | checkout → define versões → build `Release` → `dotnet test` → `dotnet pack` → `dotnet nuget push` para nuget.org |
| Segredo | `NUGET_TOKEN` |

Como a versão vem do número da execução, **não há versão no `.csproj` nem tags de release**: um `dotnet pack` local gera `1.0.0`. O *patch* cresce a cada execução do workflow, inclusive as que falham.

As versões são gravadas em `$GITHUB_ENV` (`VERSION_PROJECT` e `VERSION_PACKAGE`) e a telemetria do CLI fica desativada (`DOTNET_CLI_TELEMETRY_OPTOUT: true`).

### Pendências conhecidas do workflow

- Não há CI em push/PR: build e testes só rodam no momento da publicação.
- Execuções que falham consomem um número de versão (foi o que aconteceu com a `1.0.4`).

### Metadados do pacote

O `.csproj` define `Title`, `Authors`, `Company`, `Description`, `Copyright` e `PackageProjectUrl`. **Não** define `PackageLicenseExpression`, `PackageReadmeFile`, `RepositoryUrl`, `PackageTags` nem `GenerateDocumentationFile` — o pacote é publicado sem licença declarada, sem README embutido e sem IntelliSense de documentação.

## Outros arquivos

- `src/nuget.config` — define `repositoryPath=..\packages` (configuração herdada do formato `packages.config`; sem efeito com `PackageReference`).
- `.gitignore` — contém marcadores de conflito de merge não resolvidos (`<<<<<<< HEAD` / `=======` / `>>>>>>>`) em vários blocos. O Git trata essas linhas como padrões literais; o arquivo funciona, mas precisa de limpeza.
- `LICENSE` — MIT, © 2019 Leandro Ferreira.
- [`CHANGELOG.md`](../CHANGELOG.md) — histórico de versões; atualize a seção "Não publicado" a cada mudança.
- Não existem `CONTRIBUTING`, `.editorconfig` nem `Directory.Build.props`.

## Convenções

- Código organizado com `#region` (`Constructors`, `Properties`, `Methods`, ...).
- Commits, comentários e documentação em português (PT-BR); histórico recente usa prefixos no estilo *Conventional Commits* (`fix(filter):`, `docs:`).
- Ramo principal: `master`.
