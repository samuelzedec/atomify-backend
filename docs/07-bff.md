# BFF — Backend for Frontend

> Camada intermediária entre os consumers (Angular admin e apps externas) e o núcleo do AuthPlatform. Cada BFF é moldado para as necessidades específicas do seu consumer.

---

## Por que BFF?

O AuthPlatform tem dois consumers com necessidades completamente diferentes:

| Consumer | O que precisa |
|---|---|
| Painel Admin Angular | Dados agregados, métricas, gestão de usuários e tenants |
| Apps externas (suas apps) | Fluxos OAuth2/OIDC, tokens, login social |

Um único API tentando servir os dois vira um caos — endpoints misturados, payloads genéricos demais, lógica acoplada. O padrão BFF resolve isso criando uma API dedicada por consumer.

---

## Arquitetura com BFF

```
                   [ AuthPlatform Core ]
              Modules: Identity, OAuth, Audit...
                          ↑         ↑
           ┌──────────────┘         └──────────────┐
           │                                       │
    [ BFF Admin ]                          [ BFF Public ]
    /bff/admin/*                           /bff/public/*
           │                                       │
  [ Angular Admin Panel ]            [ Suas apps externas ]
    Painel de gestão                  Angular, APIs, mobile
```

---

## BFF Admin

Serve exclusivamente o painel Angular de administração. Nunca é acessado pelas apps externas.

### Responsabilidades

- Agregar dados de múltiplos módulos em uma única resposta
- Formatar payloads exatamente como o Angular precisa — sem transformação no frontend
- Autenticar e autorizar apenas usuários com role `SuperAdmin`
- Proteger endpoints sensíveis de gestão da plataforma

### Endpoints principais

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/bff/admin/dashboard` | Métricas gerais + logins recentes + usuários ativos — tudo numa chamada |
| `GET` | `/bff/admin/tenants` | Lista paginada de tenants com contagem de usuários |
| `POST` | `/bff/admin/tenants` | Criação de novo tenant com settings padrão |
| `GET` | `/bff/admin/tenants/{id}` | Detalhes completos do tenant: settings, limites, métricas |
| `PUT` | `/bff/admin/tenants/{id}/settings` | Atualiza políticas de senha, MFA, sessão |
| `GET` | `/bff/admin/tenants/{id}/users` | Lista de usuários do tenant com roles |
| `POST` | `/bff/admin/tenants/{id}/users/invite` | Envia convite de usuário |
| `PUT` | `/bff/admin/tenants/{id}/users/{userId}/roles` | Atribui ou remove roles |
| `PUT` | `/bff/admin/tenants/{id}/users/{userId}/deactivate` | Desativa usuário |
| `GET` | `/bff/admin/tenants/{id}/clients` | Lista de apps OAuth cadastradas |
| `POST` | `/bff/admin/tenants/{id}/clients` | Cadastra nova app OAuth |
| `GET` | `/bff/admin/tenants/{id}/audit` | Log de auditoria filtrado e paginado |

### Autenticação

Usa JWT com claim `role = SuperAdmin`. Qualquer requisição sem essa claim retorna `403 Forbidden`.

### Exemplo de agregação

Um único `GET /bff/admin/dashboard` retorna:

```json
{
  "totalTenants": 3,
  "totalUsers": 142,
  "loginsToday": 38,
  "failedLoginsToday": 4,
  "recentAuditLogs": [...],
  "topTenantsByUsers": [...],
  "mfaAdoptionRate": 0.61
}
```

Sem o BFF, o Angular teria que fazer 4 chamadas separadas e montar isso no frontend.

---

## BFF Public

Serve as apps externas que usam o AuthPlatform como IdP. É a superfície pública exposta à internet.

### Responsabilidades

- Expor os fluxos OAuth2/OIDC para as apps externas
- Emitir, renovar e revogar tokens
- Expor endpoints padrão OIDC (authorize, token, userinfo, jwks)
- Aplicar rate limiting agressivo — é o endpoint mais exposto

### Endpoints principais

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/bff/public/oauth/authorize` | Inicia o Authorization Code Flow. Redireciona para tela de login |
| `POST` | `/bff/public/oauth/token` | Troca code por Access Token + Refresh Token |
| `POST` | `/bff/public/oauth/token/refresh` | Renova o Access Token com o Refresh Token |
| `POST` | `/bff/public/oauth/token/revoke` | Revoga um token explicitamente |
| `GET` | `/bff/public/oauth/userinfo` | Retorna dados do usuário autenticado (OIDC) |
| `GET` | `/bff/public/.well-known/openid-configuration` | Discovery document — descreve o IdP |
| `GET` | `/bff/public/.well-known/jwks.json` | Chaves públicas para validar os JWTs |
| `POST` | `/bff/public/auth/register` | Registro de novo usuário |
| `POST` | `/bff/public/auth/login` | Login com e-mail e senha |
| `GET` | `/bff/public/auth/social/{provider}` | Inicia login social (Google, GitHub) |
| `GET` | `/bff/public/auth/social/{provider}/callback` | Callback do provedor social |

### Autenticação

Os endpoints de fluxo OAuth validam `client_id` + `client_secret` ou PKCE. Endpoints autenticados validam o JWT no header `Authorization: Bearer {token}`.

### Rate Limiting aplicado

| Endpoint | Limite |
|---|---|
| `POST /auth/login` | 10 req / minuto por IP |
| `POST /oauth/token` | 30 req / minuto por client |
| `GET /oauth/authorize` | 20 req / minuto por IP |
| Demais endpoints | 100 req / minuto por IP |

---

## O que os BFFs NÃO fazem

- Não contêm regras de negócio — isso fica nos módulos do core
- Não acessam o banco diretamente — chamam os módulos via interfaces do SharedKernel
- Não se comunicam entre si — BFF Admin e BFF Public são completamente independentes

---

## Fluxo completo: sua app Angular usando o AuthPlatform

```
1. Usuário clica "Entrar com Google" na sua app
2. Sua app redireciona para:
   BFF Public → /bff/public/auth/social/google

3. BFF Public redireciona para o Google OAuth
4. Google autentica e chama o callback:
   BFF Public → /bff/public/auth/social/google/callback

5. BFF Public repassa para o módulo Identity
6. Identity cria ou localiza o User + ExternalLogin
7. Módulo OAuth gera Access Token + Refresh Token
8. BFF Public retorna os tokens para sua app

9. Sua app armazena os tokens
10. Usa o Access Token nas chamadas para suas APIs
```

---

## Estrutura de pastas

```
AuthPlatform/
└── src/
    ├── BFF/
    │   ├── AuthPlatform.BFF.Admin/
    │   │   ├── Endpoints/
    │   │   ├── Middlewares/
    │   │   └── Program.cs
    │   └── AuthPlatform.BFF.Public/
    │       ├── Endpoints/
    │       ├── Middlewares/
    │       └── Program.cs
    └── Modules/
        ├── Identity/
        ├── OAuth/
        └── ...
```

Cada BFF é um projeto separado com seu próprio `Program.cs`, mas referencia os módulos do core via interfaces do SharedKernel.