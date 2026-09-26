# Ledger

API REST de livro-razão (ledger) em .NET 10 com contas, lançamentos em partidas dobradas e autenticação por usuário. Projeto de estudo com foco em regras de um domínio financeiro: lançamento nunca é alterado nem apagado, e quem errou corrige com um novo lançamento.

## Funcionalidades

- Cadastro e gestão de **contas**, cada uma pertencente a um usuário.
- **Lançamentos em partidas dobradas**: toda transação gera um par de lançamentos (débito na origem e crédito no destino) com o mesmo `IdTransacao`.
- **Lançamentos de correção**: um novo par de lançamentos que referencia o original (`IdLancamentoReferencia`), sem editar nem apagar o histórico.
- **Autenticação e autorização** com ASP.NET Core Identity (token Bearer) e duas visões de acesso: usuário comum e `Admin`.
- Erros esperados tratados com **FluentResults** e traduzidos para códigos HTTP no controller.

## Tecnologias

- .NET 10 / ASP.NET Core (Controllers)
- Entity Framework Core 10 com PostgreSQL (Npgsql)
- ASP.NET Core Identity (`IdentityDbContext<Usuario>`, endpoints de autenticação por Bearer token)
- AutoMapper (entidades e DTOs)
- FluentResults (resultado de operações)
- OpenAPI nativo do .NET
- [Scalar](https://scalar.com) para a documentação interativa da API

## Estrutura

```
Ledger/
├── Controllers/     # ContaController, LancamentoController
├── Services/        # regras de negócio (IContaService, ILancamentoService)
├── Data/            # LedgerContext, DTOs e profiles do AutoMapper
├── Models/          # entidades (Conta, Lancamento, Usuario) e extensões
├── Results/         # erros tipados do FluentResults
├── Migrations/      # migrations do EF Core
└── Program.cs       # configuração, Identity e seed da role Admin
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
| GET | `/Conta` | Lista as contas (todas, se `Admin`) |
| GET | `/Conta/{numero}` | Busca uma conta pelo número |
| PUT | `/Conta/{numero}` | Atualiza o nome da conta |
| DELETE | `/Conta/{numero}` | Exclui uma conta |

### Lançamentos

| Método | Rota | Descrição |
| --- | --- | --- |
| POST | `/Lancamento` | Cria uma transferência (par de lançamentos) |
| GET | `/Lancamento` | Lista os lançamentos |
| GET | `/Lancamento/{id}` | Busca um lançamento pelo id |
| GET | `/Lancamento/transacao/{id}` | Busca o par de uma transação (`Admin`) |
| POST | `/Lancamento/correcao` | Cria um lançamento de correção (`Admin`) |

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

```bash
dotnet run --project Ledger --launch-profile http
```

A API sobe em `http://localhost:5106`.

### Documentação interativa (Scalar)

No ambiente de desenvolvimento a API expõe a documentação com o [Scalar](https://scalar.com), onde é possível ler e testar todos os endpoints pelo navegador:

- Interface: `http://localhost:5106/scalar/v1`
- Documento OpenAPI (JSON): `http://localhost:5106/openapi/v1.json`

### Primeiro uso

1. `POST /auth/register` com e-mail e senha.
2. `POST /auth/login` e copie o `accessToken` da resposta.
3. Envie o token no header `Authorization: Bearer <accessToken>` nas demais requisições.
4. Para virar `Admin`, registre o e-mail configurado em `AdminEmail`, reinicie a API (o seed roda na inicialização) e faça login de novo, pois a role entra no token no momento do login.
