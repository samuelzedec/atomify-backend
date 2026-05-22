# SharedKernel — Comunicação entre Módulos

> Define os contratos, eventos e abstrações compartilhadas entre os módulos. É a "língua comum" da plataforma.

---

## Regra fundamental

> **Módulos nunca se chamam diretamente.**
> Um módulo não pode ter referência à classe de outro módulo.
> A comunicação acontece apenas via eventos e interfaces do SharedKernel.

---

## Por que essa regra existe?

Imagine que Identity chame diretamente uma classe do Notifications:

```
// ERRADO — Identity conhece detalhes do Notifications
var emailService = new NotificationEmailService();
emailService.SendWelcomeEmail(user.Email);
```

Isso cria acoplamento direto. Se Notifications mudar, Identity quebra. Se você quiser extrair Notifications para um microserviço no futuro, terá que reescrever Identity também.

Com eventos:

```
// CORRETO — Identity só publica um evento, não sabe quem ouve
await _eventBus.PublishAsync(new UserRegisteredEvent(user.Id, user.Email));
```

Identity não sabe que Notifications existe. Amanhã você pode adicionar um módulo de Analytics que também ouve esse evento — sem tocar em Identity.

---

## Onde o TenantId vive

O `TenantId` é um conceito do **SharedKernel** — todos os módulos precisam dele para isolar dados, mas nenhum deles conhece a entidade `Tenant` completa.

```
SharedKernel
└── TenantId  →  apenas o identificador, sem lógica de negócio

Admin Module
└── Tenant    →  entidade completa: settings, plano, limites, IsPrincipal

Identity Module
└── User.TenantId  →  referencia o ID, não a entidade Tenant

OAuth Module
└── OAuthClient.TenantId  →  idem

Audit Module
└── AuditLog.TenantId  →  idem
```

Essa separação garante que os módulos continuem independentes. O módulo Identity não precisa conhecer nada sobre planos ou configurações de tenant — ele só precisa saber a qual tenant o usuário pertence.

---

## Eventos do Sistema

Cada evento representa algo que aconteceu na plataforma. São imutáveis — descrevem o passado.

### Eventos do Identity

| Evento | Quando é disparado | Quem ouve |
|---|---|---|
| `UserRegisteredEvent` | Novo usuário criado | Notifications, Audit |
| `UserDeactivatedEvent` | Usuário desativado pelo admin | Audit |
| `LoginSucceededEvent` | Login bem-sucedido | Audit |
| `LoginFailedEvent` | Credenciais inválidas | Audit |
| `AccountLockedEvent` | Conta bloqueada por tentativas | Notifications, Audit |
| `PasswordChangedEvent` | Senha alterada | Notifications, Audit |
| `MfaEnabledEvent` | MFA ativado pelo usuário | Audit |
| `MfaFailedEvent` | Código TOTP inválido | Audit |
| `ExternalLoginLinkedEvent` | Login social vinculado | Audit |

### Eventos do OAuth

| Evento | Quando é disparado | Quem ouve |
|---|---|---|
| `TokenIssuedEvent` | Access Token gerado | Audit |
| `TokenRevokedEvent` | Token revogado | Audit |
| `ClientCreatedEvent` | Nova app OAuth cadastrada | Audit |
| `ClientDeactivatedEvent` | App OAuth desativada | Audit |

### Eventos do Admin

| Evento | Quando é disparado | Quem ouve |
|---|---|---|
| `TenantCreatedEvent` | Novo tenant criado | Audit |
| `TenantDeactivatedEvent` | Tenant desativado | Audit |
| `TenantBootstrappedEvent` | Seed inicial executado com sucesso | Audit |

### Eventos do Notifications

| Evento | Quando é disparado | Quem ouve |
|---|---|---|
| `UserInviteCreatedEvent` | Convite gerado pelo admin | Notifications, Audit |
| `InviteAcceptedEvent` | Convite aceito | Identity (cria o User), Audit |

---

## Estrutura de um Evento

Todo evento segue o mesmo contrato base:

| Propriedade | Tipo | Descrição |
|---|---|---|
| `EventId` | `Guid` | Identificador único do evento. Permite deduplicação |
| `OccurredAt` | `DateTime` | Quando o evento aconteceu — sempre UTC |
| `TenantId` | `Guid` | Tenant de contexto |
| `CorrelationId` | `Guid` | ID que conecta eventos de uma mesma operação. Útil para rastrear um fluxo completo nos logs |

---

## Interfaces compartilhadas

Além dos eventos, o SharedKernel define interfaces que os módulos implementam e consomem sem se conhecer diretamente.

### ICurrentUserContext

Disponibiliza o usuário autenticado da requisição atual para qualquer módulo que precise.

| Membro | Descrição |
|---|---|
| `UserId` | ID do usuário logado |
| `TenantId` | Tenant da requisição |
| `Email` | E-mail do usuário |
| `Roles` | Lista de roles do usuário |
| `IsAuthenticated` | Se a requisição está autenticada |
| `SystemRole` | Role do sistema do usuário: `SuperAdmin`, `TenantAdmin` ou `User` |
| `IsSuperAdmin` | Atalho — verdadeiro quando `SystemRole == SuperAdmin`. Usado para proteger endpoints globais |

### ITenantResolver

Resolve o tenant atual a partir da requisição (subdomínio, header ou claim do token).

| Membro | Descrição |
|---|---|
| `ResolveAsync()` | Retorna o `TenantId` da requisição atual |
| `IsPrincipalTenant()` | Verifica se a requisição vem do contexto do TenantPrincipal |

### IEventBus

Abstração de publicação de eventos. A implementação concreta pode ser em memória (monolito) ou via RabbitMQ (quando extrair para microserviços).

| Método | Descrição |
|---|---|
| `PublishAsync(event)` | Publica um evento para todos os ouvintes registrados |

### IBootstrapService

Interface implementada pelo módulo Admin e chamada na inicialização da aplicação para executar o seed do TenantPrincipal e SuperAdmin.

| Método | Descrição |
|---|---|
| `SeedAsync()` | Verifica e cria TenantPrincipal + SuperAdmin se ainda não existirem |

---

## Como a comunicação evolui para microserviços

No monolito modular, o `IEventBus` é implementado em memória — o evento é processado na mesma requisição ou em background local.

Quando você quiser extrair Notifications para um microserviço:

```
Antes (monolito):
Identity → IEventBus (em memória) → Notifications (mesmo processo)

Depois (microserviços):
Identity → IEventBus (RabbitMQ) → fila → Notifications (processo separado)
```

A mudança é **apenas na implementação do IEventBus**. Identity e Notifications não mudam nada — continuam publicando e ouvindo os mesmos eventos.

Essa é a vantagem de ter feito o modular monolith do jeito certo desde o início.