# Ledger

API REST de livro-razão (ledger) em .NET 10 com contas, lançamentos em partidas dobradas e autenticação por usuário. Projeto de estudo com foco em regras de um domínio financeiro: lançamento nunca é alterado nem apagado, e quem errou corrige com um novo lançamento.

## Funcionalidades

- Cadastro e gestão de **contas**, cada uma pertencente a um usuário.
- **Encerramento de contas**: uma conta com histórico nunca é apagada, é **encerrada** (`DataEncerramento` preenchida). O encerramento exige saldo zero, e uma conta encerrada não envia nem recebe lançamentos (nem de correção). A exclusão de verdade só é permitida para contas sem nenhum lançamento.
- **Saldo derivado**: nunca é uma coluna gravada, é sempre calculado como a soma dos lançamentos da conta (na busca individual e na listagem, sem N+1 — uma única consulta agrupada).
- **Lançamentos em partidas dobradas**: toda transação gera um par de lançamentos (débito na origem e crédito no destino) com o mesmo `IdTransacao`.
- **Validação de saldo suficiente**: uma transferência é recusada com `400` se a conta de origem não tiver saldo para cobrir o valor, independente de quem está operando (regra vale também para `Admin`).
- **Lançamentos de correção**: um novo par de lançamentos que referencia o original (`IdLancamentoReferencia`), sem editar nem apagar o histórico.
- **Datas definidas pelo servidor**: o cliente não envia a data. `DataTransacao` e `DataGravacao` são preenchidas pela API no momento do registro.
- **Validação de entrada** nos DTOs, com atributos próprios para CPF (dígitos verificadores) e para o limite de duas casas decimais no valor (ver [Regras de validação](#regras-de-validação)).
- **Autenticação e autorização** com ASP.NET Core Identity (token Bearer) e duas visões de acesso: usuário comum e `Admin`.
- **Tratamento de erros padronizado**: erros esperados usam FluentResults e são traduzidos para códigos HTTP num lugar só; exceções inesperadas são capturadas por um handler global. Toda resposta de erro segue o formato `ProblemDetails` (RFC 7807).

## Tecnologias

- .NET 10 / ASP.NET Core (Controllers)
- Entity Framework Core 10 com PostgreSQL (Npgsql)
- ASP.NET Core Identity (`IdentityDbContext<Usuario>`, endpoints de autenticação por Bearer token)
- Repository + Unit of Work (`Repository<T>` genérico para `Conta`; `LancamentoRepository` próprio, sem `Atualizar`/`Deletar`, respeitando a regra de lançamento imutável)
- AutoMapper (entidades e DTOs)
- FluentResults (resultado de operações, com erros tipados)
- `IExceptionHandler` + `ProblemDetails` para tratamento global de exceções
- [Swashbuckle / Swagger UI](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) para a documentação interativa da API, com autenticação Bearer configurada

## Estrutura

```
Ledger/
├── Controllers/        # ContaController, LancamentoController, ApiControllerBase
├── Handlers/           # GlobalExceptionHandler (exceções inesperadas -> ProblemDetails 500)
├── Services/           # regras de negócio (IContaService, ILancamentoService)
├── Data/
│   ├── Dtos/           # DTOs de Conta e Lancamento
│   ├── Profiles/       # profiles do AutoMapper
│   ├── Repositories/   # Repository<T>, ContaRepository, LancamentoRepository
│   │   └── Interfaces/
│   ├── UnitOfWork/     # IUnitOfWork / UnitOfWork
│   └── LedgerContext.cs
├── Models/             # entidades (Conta, Lancamento, Usuario) e extensões
├── Results/            # erros tipados do FluentResults
├── Validation/         # atributos de validação (CpfAttribute, MaximoDuasCasasDecimaisAttribute)
├── Migrations/         # migrations do EF Core
└── Program.cs          # configuração, Identity, Swagger e seed da role Admin
```

## Endpoints

Todos os endpoints de `Conta` e `Lancamento` exigem autenticação (`Authorization: Bearer <token>`).

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
| GET | `/Conta` | Lista as contas com saldo e `dataEncerramento` (todas, se `Admin`) |
| GET | `/Conta/{numero}` | Busca uma conta pelo número, com saldo e `dataEncerramento` |
| PUT | `/Conta/{numero}` | Atualiza o nome da conta |
| POST | `/Conta/{numero}/encerrar` | Encerra a conta (exige saldo zero e conta ainda ativa) |
| DELETE | `/Conta/{numero}` | Exclui a conta, **somente se ela não tiver lançamentos** (senão `409`: use o encerramento) |

### Lançamentos

| Método | Rota | Descrição |
| --- | --- | --- |
| POST | `/Lancamento` | Cria uma transferência (par de lançamentos); a data é definida pelo servidor |
| GET | `/Lancamento` | Lista os lançamentos |
| GET | `/Lancamento/{id}` | Busca um lançamento pelo id |
| GET | `/Lancamento/transacao/{id}` | Busca o par de uma transação (`Admin`) |
| POST | `/Lancamento/correcao` | Cria um lançamento de correção (`Admin`) |

## Regras de validação

| Campo | Regra |
| --- | --- |
| `Conta.nome` | Obrigatório, até 250 caracteres |
| `Conta.cpf` | Obrigatório, 11 dígitos numéricos (sem pontuação), com dígitos verificadores válidos e sem todos os dígitos iguais |
| `Lancamento.numeroContaOrigem` / `numeroContaDestino` | Inteiros maiores que zero e diferentes entre si |
| `Lancamento.valor` | Maior que zero e com no máximo duas casas decimais |
| `Lancamento.descricao` | Opcional, até 250 caracteres |
| `Correcao.idLancamentoReferencia` | Inteiro maior que zero |

Além da validação dos campos, valem estas regras de negócio nas transferências e nas contas:
- A transferência é recusada (`400`) quando a conta de origem não tem saldo suficiente.
- A transferência e o lançamento de correção são recusados (`409`) quando alguma das contas envolvidas está encerrada.
- O encerramento é recusado (`409`) quando a conta já está encerrada ou quando o saldo não é zero.
- Uma conta encerrada não é reaberta: não existe operação de reabertura por enquanto.

## Respostas de erro

Todas as respostas de erro usam o formato `ProblemDetails` (RFC 7807):

| Status | Quando acontece |
| --- | --- |
| `400` | Campo inválido (validação dos DTOs) ou saldo insuficiente na conta de origem |
| `401` / `403` | Token ausente/inválido ou sem permissão para o recurso |
| `404` | Conta ou lançamento não encontrado (inclui conta de outro usuário, para não revelar que ela existe) |
| `409` | Conflito com o estado atual: conta com lançamentos que não pode ser excluída, conta já encerrada, encerramento com saldo diferente de zero, ou movimentação envolvendo conta encerrada |
| `500` | Erro inesperado: o detalhe fica só no log do servidor, a resposta é genérica |

## Como rodar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL em execução
- Ferramenta do EF Core: `dotnet tool install --global dotnet-ef`

### Configuração

Os dados sensíveis ficam no [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), fora do repositório. Na pasta do projeto (`Ledger/Ledger`):

```bash
dotnet user-secrets set "ConnectionStrings:LedgerConnection" "Host=localhost;Port=5432;Database=Ledger;Username=SEU_USUARIO;Password=SUA_SENHA"
dotnet user-secrets set "AdminEmail" "seu-email@exemplo.com"
```

`AdminEmail` é o e-mail que será promovido à role `Admin` na inicialização da API. O usuário precisa já estar registrado.

### Banco de dados

Na raiz do repositório:

```bash
dotnet ef database update --project Ledger
```

### Execução

Na primeira vez, confia no certificado HTTPS de desenvolvimento (evita erro de "Failed to fetch" no Swagger):

```bash
dotnet dev-certs https --trust
```

Depois, sobe a API no profile `https`, que expõe a porta HTTPS usada pelo Swagger:

```bash
dotnet run --project Ledger --launch-profile https
```

A API sobe em `https://localhost:7251` (e também em `http://localhost:5106`).

### Documentação interativa (Swagger)

No ambiente de desenvolvimento a API expõe a documentação com Swagger UI, onde é possível ler e testar todos os endpoints pelo navegador:

- Interface: `https://localhost:7251/swagger`
- Documento OpenAPI (JSON): `https://localhost:7251/swagger/v1/swagger.json`

### Primeiro uso

1. `POST /auth/register` com e-mail e senha.
2. `POST /auth/login` e copie o `accessToken` da resposta.
3. No Swagger, clique em **Authorize** (canto superior direito) e cole só o token, sem o prefixo `Bearer` — o esquema já adiciona sozinho. Em outro cliente (Postman, etc.), envie no header `Authorization: Bearer <accessToken>`.
4. Para virar `Admin`, registre o e-mail configurado em `AdminEmail`, reinicie a API (o seed roda na inicialização) e faça login de novo, pois a role entra no token no momento do login.
