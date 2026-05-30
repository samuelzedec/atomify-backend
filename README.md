# Atomify — Backend

Identity Provider moderno, self-hosted, construído como Modular Monolith com ASP.NET Core 10.

---

## Stack

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| Banco de dados | PostgreSQL (EF Core 10 + Npgsql) |
| Cache | Redis (HybridCache + StackExchange.Redis) |
| Mediator | Mediator (source-generated) |
| Validação | FluentValidation 12 |
| Documentação de API | Scalar |
| Rate limiting | RedisRateLimiting |
| Testes | xUnit v3, FluentAssertions, NSubstitute, Bogus |

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (`>= 10.0.100`)
- PostgreSQL rodando e acessível
- Redis rodando e acessível
- Docker (opcional, para subir as dependências localmente)

Verifique a versão do SDK exigida em `global.json`:

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestMajor"
  }
}
```

---

## Estrutura do projeto

```
atomify-backend/
├── src/
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks.Application      # Abstrações de commands, queries e behaviors
│   │   ├── BuildingBlocks.Infrastructure   # Persistência, repositórios, mapeamentos
│   │   └── BuildingBlocks.SharedKernel     # Interfaces, exceções e contratos compartilhados
│   ├── Modules/
│   │   └── Identity/
│   │       ├── Identity.Domain             # Entidades, value objects e enums
│   │       ├── Identity.Application        # Casos de uso (commands/queries)
│   │       └── Identity.Infrastructure     # EF Core, repositórios concretos
│   └── Hosts/
│       └── Admin.Api                       # Entry point — API HTTP
├── tests/
│   └── Identity/
│       └── Identity.UnitTests
├── docs/                                   # Documentação de cada módulo
├── Directory.Packages.props                # Central Package Management
├── Directory.Build.props                   # Configurações globais de build
└── Atomify.slnx                            # Solution file
```

---

## Configuração

### 1. Ferramentas locais

Instale as ferramentas .NET do projeto (EF Core CLI, cobertura, SonarScanner):

```bash
dotnet tool restore
```

### 2. Variáveis de configuração

O projeto usa **User Secrets** em desenvolvimento. Configure os segredos da `Admin.Api`:

```bash
cd src/Hosts/Admin.Api

# String de conexão com o PostgreSQL
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=atomify;Username=postgres;Password=postgres"

# Conexão com o Redis
dotnet user-secrets set "Redis:Configuration" "localhost:6379"
```

O `appsettings.json` já contém a estrutura esperada:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Redis": {
    "Configuration": "",
    "InstanceName": "admin:app:"
  }
}
```

### 3. Variáveis de ambiente — primeiro boot

Na primeira execução, o sistema cria automaticamente o tenant principal e o SuperAdmin com base nas variáveis abaixo:

```env
AUTHPLATFORM_ADMIN_EMAIL=seu@email.com
AUTHPLATFORM_ADMIN_PASSWORD=SenhaForte123!
AUTHPLATFORM_PRINCIPAL_TENANT_NAME=Atomify
AUTHPLATFORM_PRINCIPAL_TENANT_SLUG=atomify
```

Se os registros já existirem no banco, o seed é ignorado.

---

## Banco de dados

Aplique as migrations com o EF Core CLI:

```bash
dotnet ef database update --project src/Modules/Identity/Identity.Infrastructure --startup-project src/Hosts/Admin.Api
```

Para criar uma nova migration:

```bash
dotnet ef migrations add <NomeDaMigration> \
  --project src/Modules/Identity/Identity.Infrastructure \
  --startup-project src/Hosts/Admin.Api
```

---

## Rodando a API

```bash
dotnet run --project src/Hosts/Admin.Api
```

A documentação interativa estará disponível em:

```
http://localhost:<porta>/scalar
```

Os health checks ficam em:

```
http://localhost:<porta>/health
```

---

## Testes

```bash
# Todos os testes
dotnet test

# Com relatório de cobertura
dotnet coverage collect dotnet test --output coverage --output-format cobertura
```

---

## Build

```bash
dotnet build
```

O projeto tem `TreatWarningsAsErrors=true` e `EnforceCodeStyleInBuild=true` — warnings quebram o build.

---

## Gerenciamento de pacotes

O projeto usa **Central Package Management**. Versões são declaradas somente em `Directory.Packages.props`. Nos `.csproj` individuais, referencie o pacote sem versão:

```xml
<PackageReference Include="FluentValidation" />
```

---

## Documentação dos módulos

| Arquivo | Conteúdo |
|---|---|
| [docs/00-visao-geral.md](docs/00-visao-geral.md) | Arquitetura geral e decisões de design |
| [docs/01-identity-module.md](docs/01-identity-module.md) | Usuários, senhas, MFA |
| [docs/02-oauth-module.md](docs/02-oauth-module.md) | Clients, tokens, OAuth2/OIDC |
| [docs/03-notifications-module.md](docs/03-notifications-module.md) | E-mails e alertas |
| [docs/04-audit-module.md](docs/04-audit-module.md) | Log de ações sensíveis |
| [docs/05-admin-module.md](docs/05-admin-module.md) | Tenants, métricas e bootstrap |
| [docs/06-shared-kernel.md](docs/06-shared-kernel.md) | Contratos e comunicação entre módulos |
| [docs/07-bff.md](docs/07-bff.md) | Backend for Frontend |
