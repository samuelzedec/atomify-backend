# AuthPlatform — Visão Geral

> Identity Provider moderno, self-hosted, construído com .NET + Angular.

---

## O que é

O AuthPlatform centraliza autenticação e autorização para todas as suas aplicações. Em vez de reimplementar login, MFA e refresh token em cada projeto, você pluga suas apps nele e ele cuida de tudo.

```
[ Angular Admin ]          [ Suas apps externas ]
       │                           │
 [ BFF Admin ]              [ BFF Public ]
       └──────────┬──────────────┘
                  │
        [ AuthPlatform Core ]
    Modules: Identity, OAuth, Audit...
```

---

## Estrutura de Módulos

| Módulo | Responsabilidade |
|---|---|
| [Identity](./01-identity.md) | Usuários, senhas, MFA, login social |
| [OAuth](./02-oauth.md) | Clients, tokens, fluxos OAuth2/OIDC |
| [Notifications](./03-notifications.md) | E-mails, alertas, convites |
| [Audit](./04-audit.md) | Log de todas as ações sensíveis |
| [Admin](./05-admin.md) | Tenants, configurações, métricas, bootstrap |
| [SharedKernel](./06-sharedkernel.md) | Eventos, interfaces, comunicação entre módulos |
| [BFF](./07-bff.md) | Backend for Frontend — Admin e Public |

---

## Arquitetura — Modular Monolith

Um único deploy, mas com fronteiras de domínio bem definidas internamente.

```
AuthPlatform/
├── src/
│   ├── BFF/
│   │   ├── AuthPlatform.BFF.Admin/
│   │   └── AuthPlatform.BFF.Public/
│   ├── Modules/
│   │   ├── Identity/
│   │   ├── OAuth/
│   │   ├── Notifications/
│   │   ├── Audit/
│   │   └── Admin/
│   └── SharedKernel/
└── frontend/
    └── admin-panel/
```

> **Regras fundamentais:**
> - Módulos nunca se chamam diretamente — só via eventos e interfaces do SharedKernel
> - Os BFFs são os únicos pontos de entrada externos
> - O `TenantId` vive no SharedKernel — todos os módulos o conhecem, mas só o Admin conhece a entidade `Tenant` completa

---

## Bootstrap — Primeiro boot

Na primeira inicialização, antes de qualquer requisição, o sistema cria automaticamente o **TenantPrincipal** e o **SuperAdmin** a partir de variáveis de ambiente:

```env
AUTHPLATFORM_ADMIN_EMAIL=seu@email.com
AUTHPLATFORM_ADMIN_PASSWORD=SenhaForte123!
AUTHPLATFORM_PRINCIPAL_TENANT_NAME=AuthPlatform
AUTHPLATFORM_PRINCIPAL_TENANT_SLUG=authplatform
```

O TenantPrincipal e o SuperAdmin são **imutáveis** — nenhuma operação da API pode apagá-los ou desativá-los. Se os registros já existem no banco, o seed é ignorado. Veja detalhes em [Admin](./05-admin.md).

---

## Por que Modular Monolith?

| Aspecto | Microserviços direto | Modular Monolith |
|---|---|---|
| Complexidade inicial | Alta | Baixa |
| Debug | Difícil — logs distribuídos | Simples — stack trace único |
| Fronteiras de domínio | Definidas sob pressão | Provadas antes de separar |
| Evolução futura | Difícil refatorar | Extrai módulo por módulo |

---

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core 9 + OpenIddict + EF Core |
| Banco | PostgreSQL |
| Cache / Tokens | Redis |
| E-mail | MailKit |
| Frontend | Angular 19 + PrimeNG |
| Infra | Docker Compose + Caddy |
| Logs | Serilog + Seq |
