# Módulo: Admin

> Responsável por expor dados agregados, métricas e configurações para o painel Angular de administração.

---

## Responsabilidades

- Gerenciamento de Tenants (suas aplicações cadastradas)
- Bootstrap do TenantPrincipal e SuperAdmin no primeiro boot
- Métricas de uso por tenant
- Configurações globais por tenant (políticas de senha, MFA obrigatório, etc.)
- Exposição de dados consolidados para o painel Angular
- Gerenciamento de planos e limites de recursos

---

## Conceito: Tenant

Um **Tenant** representa uma aplicação cadastrada no AuthPlatform. Cada projeto seu que for usar o IdP terá seu próprio Tenant, com usuários, roles e configurações completamente isolados.

```
AuthPlatform
├── TenantPrincipal         ← tenant raiz, imutável, criado no boot
│   └── SuperAdmin          ← único usuário com acesso global
├── Tenant: Meu Portfólio
│   ├── Usuários: João, Maria
│   └── Roles: Admin, Viewer
├── Tenant: App de Finanças
│   ├── Usuários: João, Pedro
│   └── Roles: Admin, User
└── Tenant: API de Projetos
    ├── Usuários: Maria
    └── Roles: Developer
```

> O usuário João pode existir em vários tenants com e-mails e roles diferentes em cada um. Os dados nunca se misturam.

---

## Bootstrap: TenantPrincipal e SuperAdmin

Na primeira inicialização, antes de qualquer requisição ser atendida, o sistema executa um **seed automático** lendo variáveis de ambiente. Garante que sempre existirá um ponto de entrada seguro na plataforma.

### Variáveis de ambiente necessárias

```env
AUTHPLATFORM_ADMIN_EMAIL=seu@email.com
AUTHPLATFORM_ADMIN_PASSWORD=SenhaForte123!
AUTHPLATFORM_PRINCIPAL_TENANT_NAME=AuthPlatform
AUTHPLATFORM_PRINCIPAL_TENANT_SLUG=authplatform
```

### O que é criado no primeiro boot

```
TenantPrincipal
└── IsPrincipal = true  →  nunca pode ser apagado nem desativado
└── Agrupa o SuperAdmin e as configurações globais da plataforma

SuperAdmin (User)
└── Vinculado ao TenantPrincipal
└── SystemRole = SuperAdmin  →  único com acesso ao painel de gestão global
└── IsProtected = true  →  nunca pode ser apagado, desativado ou ter role alterada
└── Senha definida via variável de ambiente
```

Se os registros já existem no banco, o seed é ignorado completamente. **Idempotente por design.**

### Proteções aplicadas em runtime

Qualquer tentativa de burlar via API é bloqueada:

| Ação bloqueada | Motivo |
|---|---|
| Deletar Tenant onde `IsPrincipal = true` | Quebraria o acesso total à plataforma |
| Desativar Tenant onde `IsPrincipal = true` | Idem |
| Deletar User onde `IsProtected = true` | Removeria o único SuperAdmin |
| Desativar User onde `IsProtected = true` | Bloquearia o acesso ao painel admin |
| Alterar `SystemRole` de User onde `IsProtected = true` | Rebaixaria o SuperAdmin |

---

## Entidades

### Tenant

Representa uma aplicação registrada no AuthPlatform. É a entidade raiz que agrupa tudo: usuários, clients OAuth, roles e configurações.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador único do tenant |
| `Name` | `string` | Nome da aplicação. Ex: `"Meu Portfólio"` |
| `Slug` | `string` | Versão URL-friendly do nome. Ex: `"meu-portfolio"`. Usado em subdomínios ou rotas |
| `LogoUrl` | `string?` | URL do logo que aparece na tela de login personalizada |
| `PrimaryColor` | `string?` | Cor principal em hex. Permite customizar a tela de login por tenant |
| `IsActive` | `bool` | Desativar um tenant bloqueia o acesso de todos os usuários dele |
| `IsPrincipal` | `bool` | Marca o tenant raiz da plataforma. Apenas um pode existir. Bloqueia deleção e desativação |
| `Plan` | `enum` | Plano de recursos: `Free`, `Pro`, `Enterprise` |
| `CreatedAt` | `DateTime` | Quando o tenant foi criado |
| `OwnerId` | `Guid` | ID do usuário dono/criador do tenant |

---

### TenantSettings

Configurações de segurança e comportamento específicas de cada tenant. Separada do Tenant para deixar a entidade principal enxuta.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador interno |
| `TenantId` | `Guid` | FK para o tenant. Relação 1-1 |
| `MfaRequired` | `bool` | Se verdadeiro, todos os usuários são obrigados a ativar MFA |
| `PasswordMinLength` | `int` | Tamanho mínimo de senha. Padrão: 8 |
| `PasswordRequireUppercase` | `bool` | Exige ao menos uma letra maiúscula |
| `PasswordRequireNumber` | `bool` | Exige ao menos um número |
| `PasswordRequireSymbol` | `bool` | Exige ao menos um símbolo especial |
| `MaxFailedLoginAttempts` | `int` | Quantidade de erros antes de bloquear a conta. Padrão: 5 |
| `LockoutDurationMinutes` | `int` | Duração do bloqueio em minutos. Padrão: 15 |
| `SessionDurationMinutes` | `int` | Tempo de vida do Access Token. Padrão: 15 minutos |
| `RefreshTokenDurationDays` | `int` | Tempo de vida do Refresh Token. Padrão: 30 dias |
| `AllowSocialLogin` | `bool` | Habilita ou desabilita login com Google/GitHub para esse tenant |
| `AllowedSocialProviders` | `string` | Provedores sociais permitidos: `"Google,GitHub"` |

---

### TenantPlanLimit

Define os limites de recursos por plano. Permite controlar o uso por tenant sem hardcodar regras no código.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador interno |
| `TenantId` | `Guid` | FK para o tenant |
| `MaxUsers` | `int` | Número máximo de usuários ativos permitidos |
| `MaxOAuthClients` | `int` | Número máximo de aplicações OAuth cadastradas |
| `MaxRoles` | `int` | Número máximo de roles por tenant |

---

### TenantMetricSnapshot

Snapshot periódico de métricas do tenant para exibição no dashboard. Evita calcular em tempo real a cada consulta.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador interno |
| `TenantId` | `Guid` | FK para o tenant |
| `Date` | `DateOnly` | Data do snapshot — gerado uma vez por dia |
| `TotalUsers` | `int` | Total de usuários cadastrados no tenant |
| `ActiveUsers` | `int` | Usuários que fizeram login nos últimos 30 dias |
| `TotalLogins` | `int` | Total de logins bem-sucedidos no dia |
| `FailedLogins` | `int` | Total de tentativas falhas no dia |
| `NewUsers` | `int` | Usuários criados no dia |
| `MfaAdoptionRate` | `decimal` | Percentual de usuários com MFA ativo |

> **Por que snapshot e não consulta em tempo real?**
> Calcular `ActiveUsers` em tempo real exigiria varrer toda a tabela de AuditLog. Com snapshots diários, o dashboard carrega instantaneamente e os dados do dia anterior já estão calculados.

---

## Fluxo: Boot da aplicação

```
1. Aplicação inicia
2. EF Core aplica migrations pendentes
3. Seed verifica: TenantPrincipal já existe no banco?
   → Sim: ignora e continua
   → Não: lê as variáveis de ambiente e cria:
       - TenantPrincipal (IsPrincipal = true)
       - TenantSettings padrão para o TenantPrincipal
       - SuperAdmin (IsProtected = true, Role = SuperAdmin)
4. Aplicação começa a atender requisições
```

---

## Fluxo: Criação de um novo Tenant

```
1. SuperAdmin loga no painel admin
2. Acessa "Aplicações" → "Nova Aplicação"
3. Preenche: nome, slug, plano, logo (opcional)
4. Sistema valida: slug único no banco?
5. Sistema cria:
   → Tenant (IsPrincipal = false)
   → TenantSettings com valores padrão
   → TenantPlanLimit conforme o plano escolhido
   → OAuthClient gerado automaticamente (ClientId + ClientSecret)
6. SuperAdmin recebe o ClientId e ClientSecret gerados
7. Configura as RedirectUris da app
8. Pluga o ClientId na app e começa a usar
```

---

## Fluxo: Dashboard de métricas no painel Angular

```
1. Admin abre o painel
2. Angular chama GET /bff/admin/dashboard
3. BFF Admin agrega dados: busca TenantMetricSnapshot + AuditLog recente
4. Retorna tudo numa única resposta: total de usuários, logins de hoje, falhas, adoção de MFA
5. Angular renderiza os gráficos e cards de métricas
6. Um job agendado (Hangfire) roda todo dia à meia-noite
   e gera o snapshot do dia seguinte
```

---

## O que o painel Angular expõe

Todos os dados são consumidos via **BFF Admin** (`/bff/admin/*`). O Angular nunca acessa os módulos do core diretamente.

| Seção | O que mostra |
|---|---|
| Dashboard | Métricas gerais, gráfico de logins por dia, usuários ativos |
| Aplicações | Lista de Tenants, criação, edição, ativação/desativação |
| Usuários | Lista por tenant, criação, edição, ativação, atribuição de roles |
| Roles | CRUD de roles por tenant |
| Configurações | TenantSettings — políticas de senha, MFA, sessão |
| Audit Log | Histórico filtrado de ações — quem fez o quê e quando |
| Clients OAuth | Lista de apps cadastradas, geração de novo secret |