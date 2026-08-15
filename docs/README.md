# Vip.DynamicFilter — Documentação

Documentação de referência da biblioteca [Vip.DynamicFilter](../README.md).

> Todos os exemplos usam o modelo `Client` / `Address` / `ClientEnum` descrito no [README](../README.md#modelo-de-exemplo).

## Índice

| Página | Conteúdo |
|---|---|
| [filtros.md](filtros.md) | Todas as 22 condições (`WhereCondition`) com exemplos C# + JSON |
| [operadores.md](operadores.md) | Filtros compostos com `AND` / `OR` e aninhamento |
| [ordenacao.md](ordenacao.md) | `Order`, `OrderBy`, `ThenBy` e ordenação múltipla |
| [paginacao.md](paginacao.md) | `PageNumber` / `Limit` e semântica de paginação |
| [filter-response.md](filter-response.md) | `FilterResponse<T>` e `GetFilterResponse` |
| [propriedades-aninhadas.md](propriedades-aninhadas.md) | Navegação `.` e coleções (`any` / `!any`) |
| [enums-e-tipos.md](enums-e-tipos.md) | Enums e conversão automática de tipos |
| [serializacao-json.md](serializacao-json.md) | Deserialização JSON (`System.Text.Json` / `Newtonsoft.Json`) |
| [ef-core.md](ef-core.md) | Uso com EF Core |
| [limites-e-comportamentos.md](limites-e-comportamentos.md) | Exceções, defaults e edge cases |

## Convenções dos exemplos

- **C#**: usa os construtores e *object initializers* da API pública.
- **JSON**: usa nomes em **camelCase** e os **tokens** de condição (`"="`, `"~~"`, `"and"`, `"desc"`, ...), conforme o padrão de serialização descrito em [serializacao-json.md](serializacao-json.md).
- **Código equivalente**: cada exemplo mostra o que o filtro gera em LINQ padrão, para fins de comparação.
