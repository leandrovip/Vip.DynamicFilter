# CLAUDE.md

As diretrizes do projeto (estrutura, comandos, regras de código, convenções de documentação e de testes) ficam em `AGENTS.md`, que é a fonte única para todos os agentes:

@AGENTS.md

## Específico do Claude Code

- **Antes de alterar `src/`**: leia `docs/arquitetura.md` e a seção "Comportamentos que surpreendem" do `AGENTS.md`. O motor tem ~400 linhas em dois arquivos (`Expressions/WhereExpression.cs`, `Expressions/OrderExpression.cs`) — leia-os inteiros em vez de buscar trechos.
- **Depois de alterar `src/`**: rode `dotnet test ./src/Vip.DynamicFilter.sln`. Testes e demo exigem o SDK .NET 10.
- **Ao documentar comportamento**: verifique executando (um teste novo ou um console temporário fora do repositório referenciando `src/Vip.DynamicFilter/Vip.DynamicFilter.csproj`) em vez de inferir pela leitura do código. Várias afirmações "óbvias" são falsas nesta base — por exemplo, a cultura usada na conversão e o tipo de exceção de cada caso.
- **Mudança de comportamento ≠ documentação**: corrigir uma das dívidas listadas em `docs/arquitetura.md` ou `docs/matriz-de-comportamentos.md` altera o contrato do pacote publicado. Proponha e aguarde confirmação; não faça junto com outra tarefa.
- **Não toque sem pedido explícito**: `.gitignore` (marcadores de conflito), `.github/workflows/nuget.yml`, metadados de versão/pacote no `.csproj`.
- **Commits**: mensagens em PT-BR, no estilo do histórico (`docs: ...`, `fix(filter): ...`). Só faça commit quando solicitado; o ramo principal é `master`.
