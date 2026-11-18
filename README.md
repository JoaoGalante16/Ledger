# Ledger

API REST de livro-razão (ledger) em .NET 10 com PostgreSQL. Contas, transferências, depósitos e saques em partidas dobradas, com o saldo protegido contra operações simultâneas na mesma conta.

## Destaques

- **Partidas dobradas e histórico imutável**: toda operação grava um par débito/crédito; lançamento nunca é editado nem apagado, erro se corrige com estorno.
- **Concorrência testada**: 10 saques simultâneos numa conta com saldo para um só, e só um é aprovado (lock pessimista com `SELECT ... FOR UPDATE`).
- **Clean Architecture + DDD**: o domínio não depende de EF Core nem de ASP.NET, e a `Conta` é um agregado que protege as próprias regras.
- **224 testes**, incluindo integração com PostgreSQL de verdade via Testcontainers.
- **Erros em `ProblemDetails`** e **log estruturado** com Serilog, sem dados pessoais.

## Funcionalidades

- Contas por usuário: criar, listar, buscar, renomear, encerrar e excluir (só sem lançamentos)
- Transferência entre contas, depósito e saque
- Correção de transferências por estorno
- Listagens paginadas
- Autenticação com ASP.NET Core Identity (Bearer) e perfil `Admin`
- Documentação interativa com Swagger

## Decisões técnicas

| Decisão | Por quê | Custo |
| --- | --- | --- |
| **Saldo guardado na `Conta`**, alterado só por `Debitar`/`Creditar` | A conta aplica as regras de saldo sozinha, sem consultar o banco, e a consulta não precisa somar o extrato inteiro | O saldo pode divergir da soma dos lançamentos. Por isso os dois são gravados na mesma transação, e os testes conferem que batem |
| **Lock pessimista** (`FOR UPDATE`), sempre em ordem de número da conta | Na mesma conta o conflito é frequente; com lock otimista, muitas operações falhariam e precisariam de retry. A ordem fixa evita deadlock | Operações na mesma conta esperam em fila |
| **Conta Caixa** como contrapartida de depósito e saque | Mantém a partida dobrada sem exceção: o dinheiro sempre sai de uma conta e entra em outra | Todo depósito e saque trava a linha da Caixa, que vira um ponto de contenção |
| **Exceção de domínio** para regra violada, **Result** para resultado esperado | Invariante (saldo insuficiente, conta encerrada) não pode ser ignorada; "não encontrado" e "conflito" são respostas normais do fluxo | Dois mecanismos, unificados na resposta em `ProblemDetails` |
| **Casos de uso**: uma classe por operação, sem MediatR | Cada operação tem só as dependências que usa e muda sem afetar as outras | Mais arquivos, e alguns parecidos (`Depositar` e `Sacar`) |
| **Paginação no banco** (`Skip`/`Take`, máximo 100 por página) | Só as linhas da página saem do banco | Com milhões de linhas, paginação por cursor (keyset) seria mais rápida |
| **`404` para conta de outro usuário** | Não revela que a conta existe | — |

## Arquitetura

Quatro projetos, com as dependências apontando para dentro:

| Projeto | Responsabilidade |
| --- | --- |
| `Ledger.Domain` | Entidades (`Conta`, `Lancamento`), value objects (`Cpf`, `Valor`), exceções de domínio e interfaces dos repositórios |
| `Ledger.Application` | Casos de uso, DTOs, profiles do AutoMapper e validação de entrada |
| `Ledger.Infrastructure` | EF Core (`LedgerContext`, migrations), repositórios, Unit of Work, Identity e seeds |
| `Ledger.Api` | Controllers, tratamento global de exceções, Serilog, Swagger e configuração |

```
Ledger.Domain/
├── Entities/           # Conta (agregado) e Lancamento
├── ValueObjects/       # Cpf e Valor
├── Exceptions/         # exceções de regra de negócio
├── Enums/              # TipoOperacao
├── Constantes/         # dados da conta Caixa
└── Repositories/       # interfaces dos repositórios e do Unit of Work

Ledger.Application/
├── UseCases/           # uma classe por operação (Contas e Lancamentos)
├── Dtos/               # entrada e saída da API, incluindo a paginação
├── Profiles/           # mapeamentos do AutoMapper
├── Errors/             # erros tipados do FluentResults
├── Extensions/         # regra de acesso (dono da conta ou Admin)
└── Validation/         # atributos de validação (CPF, duas casas decimais)

Ledger.Infrastructure/
├── Data/               # LedgerContext e seeds (role Admin e conta Caixa)
├── Repositories/       # repositórios e Unit of Work
├── Identity/           # usuário do Identity
└── Migrations/         # migrations do EF Core

Ledger.Api/
├── Controllers/        # ContaController e LancamentoController
├── Handlers/           # tratamento global de exceções
├── Extensions/         # usuário logado e registro dos casos de uso
└── Program.cs          # configuração, Identity, Serilog e Swagger

Ledger.Tests/
├── Unitarios/          # domínio, casos de uso, validação e API, sem banco
└── Integracao/
    ├── BancoDeDados/   # repositórios contra PostgreSQL em container
    └── Api/            # endpoints por HTTP, incluindo concorrência
```

## Tecnologias

- .NET 10, ASP.NET Core (Controllers), Entity Framework Core 10 com PostgreSQL (Npgsql)
- ASP.NET Core Identity (Bearer token)
- Repository + Unit of Work, AutoMapper, FluentResults
- `IExceptionHandler` + `ProblemDetails`, Serilog, Swagger UI (Swashbuckle)
- Testes: xUnit v3, Moq, Bogus, Testcontainers (PostgreSQL), Respawn e `WebApplicationFactory`

## Endpoints

Todos os endpoints de `Conta` e `Lancamento` exigem `Authorization: Bearer <token>`.

### Autenticação (Identity)

| Método | Rota | Descrição |
| --- | --- | --- |
| POST | `/auth/register` | Cria um usuário |
| POST | `/auth/login` | Retorna `accessToken` e `refreshToken` |
| POST | `/auth/refresh` | Renova o token |

### Contas

| Método | Rota | Descrição |
| --- | --- | --- |
| POST | `/Conta` | Cria uma conta para o usuário logado |
| GET | `/Conta?pagina=1&tamanhoPagina=20` | Lista as contas em ordem de número (todas, se `Admin`) |
| GET | `/Conta/{numero}` | Busca uma conta pelo número |
| PUT | `/Conta/{numero}` | Renomeia a conta |
| POST | `/Conta/{numero}/encerrar` | Encerra a conta (exige saldo zero) |
| DELETE | `/Conta/{numero}` | Exclui a conta, só se ela não tiver lançamentos |

### Lançamentos

| Método | Rota | Descrição |
| --- | --- | --- |
| POST | `/Lancamento` | Transferência entre contas |
| POST | `/Lancamento/deposito` | Depósito na conta |
| POST | `/Lancamento/saque` | Saque da conta |
| GET | `/Lancamento?pagina=1&tamanhoPagina=20` | Lista os lançamentos, do mais novo para o mais antigo (todos, se `Admin`) |
| GET | `/Lancamento/{id}` | Busca um lançamento pelo id |
| GET | `/Lancamento/transacao/{id}` | Busca o par de uma transação (`Admin`) |
| POST | `/Lancamento/correcao` | Estorna uma transferência (`Admin`) |

As listagens aceitam `pagina` (padrão `1`) e `tamanhoPagina` (padrão `20`, máximo `100`) e respondem com:

```json
{ "itens": [ ... ], "pagina": 1, "tamanhoPagina": 20, "totalItens": 3, "totalPaginas": 1 }
```

## Regras de negócio e validação

Validação de entrada (`400` com `ValidationProblemDetails`):

| Campo | Regra |
| --- | --- |
| `nome` da conta | Obrigatório, até 250 caracteres |
| `cpf` | 11 dígitos, sem pontuação, com dígitos verificadores válidos |
| `valor` | Maior que zero, no máximo duas casas decimais |
| `numeroContaOrigem` / `numeroContaDestino` | Maiores que zero e diferentes entre si |
| `descricao` | Opcional, até 250 caracteres |
| `pagina` / `tamanhoPagina` | `pagina` maior que zero; `tamanhoPagina` entre 1 e 100 |

Regras de negócio:
- Saque e transferência exigem saldo suficiente, inclusive para `Admin`.
- Conta encerrada não envia nem recebe lançamentos, e não é reaberta.
- O encerramento exige saldo zero; a exclusão só é permitida para conta sem lançamentos.
- A conta Caixa não pode ser movimentada diretamente, renomeada, encerrada nem excluída.
- Depósito e saque não aceitam correção.
- As datas do lançamento são definidas pelo servidor, não pelo cliente.

## Respostas de erro

Todas no formato `ProblemDetails` (RFC 7807):

| Status | Quando |
| --- | --- |
| `400` | Validação de entrada, saldo insuficiente, valor zero ou negativo, nome inválido |
| `401` / `403` | Token ausente ou inválido, ou sem permissão |
| `404` | Conta ou lançamento não encontrado (inclui conta de outro usuário) |
| `409` | Conflito com o estado atual: conta encerrada, encerramento com saldo, exclusão de conta com lançamentos, operação na Caixa, correção de depósito ou saque |
| `500` | Erro inesperado; o detalhe fica só no log |

## Logs

- Uma linha por requisição (método, rota, status, tempo e id do usuário).
- Logs de negócio nas operações que movimentam dinheiro ou mudam a conta, com campos estruturados (`IdTransacao`, `Valor`, `NumeroConta`).
- Nunca são logados CPF, nome, e-mail, senha ou token, e um teste garante isso.
- Em Development sai em texto, com o SQL do EF Core; nos outros ambientes sai em JSON, sem o SQL.

## Testes

| Nível | Pasta | Testes | O que cobre |
| --- | --- | --- | --- |
| Unidade | `Unitarios/` | 152 | Agregado `Conta`, value objects, casos de uso (Unit of Work mockado), validação dos DTOs, tradução de erros e conteúdo dos logs |
| Integração com o banco | `Integracao/BancoDeDados/` | 30 | Repositórios contra PostgreSQL: gravação, busca, paginação, bloqueio das contas, chaves estrangeiras e a sequence `TransacaoIdSeq` |
| Integração da API | `Integracao/Api/` | 42 | Endpoints por HTTP com token: status, `ProblemDetails`, validação, permissões, isolamento entre usuários e concorrência |

Os testes de concorrência disparam requisições simultâneas e conferem o banco no fim:
- 10 saques simultâneos com saldo para um só: só um é aprovado, e o saldo bate com a soma dos lançamentos.
- Transferências cruzadas entre duas contas ao mesmo tempo: todas aprovadas, com os saldos corretos.
- Depósito e encerramento ao mesmo tempo: nenhuma conta termina encerrada com saldo.

Os testes de integração sobem um PostgreSQL em container com o Testcontainers (não usam o banco de desenvolvimento), limpam os dados com o Respawn antes de cada teste e, no caso da API, sobem a aplicação em memória com a `WebApplicationFactory`.

```bash
dotnet test
```

Precisa do **Docker em execução**. O `global.json` configura o `dotnet test` para o Microsoft Testing Platform, usado pelo xUnit v3.

## Como rodar

**Pré-requisitos:** [.NET 10 SDK](https://dotnet.microsoft.com/download), PostgreSQL e a ferramenta do EF Core (`dotnet tool install --global dotnet-ef`).

**1. Configuração.** Os dados sensíveis ficam no [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) do `Ledger.Api`. Na raiz do repositório:

```bash
dotnet user-secrets set "ConnectionStrings:LedgerConnection" "Host=localhost;Port=5432;Database=ledger;Username=SEU_USUARIO;Password=SUA_SENHA" --project Ledger.Api
dotnet user-secrets set "AdminEmail" "seu-email@exemplo.com" --project Ledger.Api
```

`AdminEmail` é o usuário promovido a `Admin` na inicialização (ele precisa já estar registrado). O user-secrets só é lido em Development; nos outros ambientes, use variáveis de ambiente (por exemplo, `ConnectionStrings__LedgerConnection`).

**2. Banco de dados:**

```bash
dotnet ef database update --project Ledger.Infrastructure --startup-project Ledger.Api
```

**3. Execução:**

```bash
dotnet dev-certs https --trust
dotnet run --project Ledger.Api --launch-profile https
```

A API sobe em `https://localhost:7251` e o Swagger fica em `https://localhost:7251/swagger`. Na inicialização, os seeds criam a role `Admin` e a conta Caixa.

**4. Primeiro uso:**
1. `POST /auth/register` e depois `POST /auth/login`; copie o `accessToken`.
2. No Swagger, clique em **Authorize** e cole o token (sem o prefixo `Bearer`). No Postman, use `Authorization: Bearer <accessToken>`.
3. Crie uma conta (`POST /Conta`) e faça um depósito (`POST /Lancamento/deposito`).
4. Para virar `Admin`, registre o e-mail do `AdminEmail`, reinicie a API e faça login de novo.
