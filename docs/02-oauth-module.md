# Módulo: OAuth

> Responsável por implementar os fluxos OAuth2/OIDC que permitem que suas aplicações usem o AuthPlatform como provedor de identidade.

---

## Responsabilidades

- Gerenciamento de aplicações clientes (suas apps)
- Geração e validação de Access Tokens (JWT)
- Geração e rotação de Refresh Tokens
- Authorization Code Flow com PKCE
- Gerenciamento de escopos e claims customizáveis
- Revogação de tokens via Redis

---

## Conceito fundamental: o que é um "Client"?

No contexto OAuth, **Client** é qualquer aplicação que vai usar o AuthPlatform para autenticar usuários. Exemplos:

- Seu portfólio Angular
- Uma API .NET que você criou
- Um app mobile futuro

Cada Client tem um `ClientId` e `ClientSecret` únicos, como se fosse um "usuário de sistema" que representa a aplicação.

---

## Entidades

### OAuthClient

Representa uma aplicação registrada no AuthPlatform. Cada app que quiser usar o IdP precisa ser cadastrada aqui.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador interno |
| `TenantId` | `Guid` | O client pertence a um tenant. Isola aplicações entre si |
| `ClientId` | `string` | Identificador público da aplicação. Enviado nas requisições OAuth. Não é segredo |
| `ClientSecret` | `string` | Segredo da aplicação. Armazenado como hash. Nunca retornado após a criação |
| `Name` | `string` | Nome amigável da aplicação. Aparece no painel admin |
| `Description` | `string?` | Descrição opcional para identificar o propósito da app |
| `IsActive` | `bool` | Permite desativar uma aplicação sem excluí-la |
| `CreatedAt` | `DateTime` | Data de registro da aplicação |

**Relacionamentos:**
- Um `OAuthClient` pertence a um `Tenant`
- Um `OAuthClient` tem várias `RedirectUri`
- Um `OAuthClient` tem vários `OAuthScope` permitidos

---

### RedirectUri

Define quais URLs o AuthPlatform aceita redirecionar após a autenticação. É uma proteção contra ataques de redirecionamento aberto.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador interno |
| `ClientId` | `Guid` | FK para o client dono dessa URI |
| `Uri` | `string` | URL completa de callback. Ex: `https://meuapp.com/auth/callback` |

> **Por que isso é importante?**
> Sem essa validação, um atacante poderia fazer o usuário logar e redirecionar o código de autorização para um site malicioso. O AuthPlatform só redireciona para URLs previamente cadastradas.

---

### OAuthScope

Define quais dados e permissões um client pode solicitar sobre o usuário.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador interno |
| `ClientId` | `Guid` | FK para o client que pode usar esse escopo |
| `Name` | `string` | Nome do escopo: `"openid"`, `"profile"`, `"email"`, `"roles"` |
| `Description` | `string?` | O que esse escopo dá acesso. Exibido na tela de consentimento |

> **O que são escopos na prática?**
> Quando sua app Angular pede login, ela declara quais escopos quer: `openid profile email roles`. O AuthPlatform valida se esse client tem permissão para pedir esses escopos e inclui as informações correspondentes no token JWT.

---

### AuthorizationCode

Código temporário gerado durante o Authorization Code Flow. Ele é trocado pelo Access Token + Refresh Token.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador interno |
| `Code` | `string` | Valor do código — string aleatória de uso único |
| `ClientId` | `Guid` | FK para o client que gerou esse código |
| `UserId` | `Guid` | FK para o usuário que autorizou |
| `RedirectUri` | `string` | URI que deve ser usada na troca. Deve bater exatamente com a usada na solicitação |
| `CodeChallenge` | `string` | Hash do PKCE Code Verifier. Proteção contra interceptação do código |
| `CodeChallengeMethod` | `string` | Método de hash usado: sempre `"S256"` (SHA-256) |
| `Scopes` | `string` | Escopos autorizados separados por espaço |
| `ExpiresAt` | `DateTime` | O código expira em 5 minutos. Após isso é inválido mesmo que não usado |
| `IsUsed` | `bool` | Garante uso único. Um código usado não pode ser trocado novamente |
| `CreatedAt` | `DateTime` | Momento da geração |

> **O que é PKCE?**
> PKCE (Proof Key for Code Exchange) é uma proteção para apps públicas como SPAs Angular. A app gera um segredo aleatório (`code_verifier`), manda o hash dele (`code_challenge`) na solicitação, e manda o valor original na troca. Isso garante que só quem iniciou o fluxo pode trocar o código.

---

### RevokedToken

Mantém a lista de Access Tokens que foram explicitamente revogados antes de expirar. Armazenado em Redis para consulta rápida.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Jti` | `string` | JWT ID — identificador único do token revogado. Presente no payload do JWT |
| `UserId` | `Guid` | Usuário dono do token revogado |
| `RevokedAt` | `DateTime` | Quando foi revogado |
| `ExpiresAt` | `DateTime` | Quando expiraria naturalmente. Usado para limpar o Redis automaticamente |

> **Por que Redis e não PostgreSQL?**
> A validação de token acontece em toda requisição autenticada. Consultar o PostgreSQL a cada request seria lento. O Redis mantém os tokens revogados em memória com TTL automático — quando o token expiraria naturalmente, o Redis já o remove sozinho.

---

## Fluxo: Authorization Code Flow com PKCE

Este é o fluxo principal usado pelo seu Angular para autenticar usuários.
Os endpoints abaixo são expostos pelo **BFF Public** — nunca acesse o módulo OAuth diretamente.

```
[ App Angular ]                    [ BFF Public → OAuth Module ]

1. Gera code_verifier (aleatório)
   code_challenge = SHA256(code_verifier)

2. Redireciona o usuário para:
   /bff/public/oauth/authorize
     ?client_id=abc
     &redirect_uri=https://meuapp.com/callback
     &response_type=code
     &scope=openid profile email
     &code_challenge=xyz
     &code_challenge_method=S256

                                   3. Exibe tela de login
                                   4. Usuário loga
                                   5. Valida client_id e redirect_uri
                                   6. Gera AuthorizationCode (5 min)
                                   7. Redireciona para:
                                      https://meuapp.com/callback?code=CODE

8. Recebe o code na callback
9. Faz POST para /bff/public/oauth/token com:
   - code=CODE
   - code_verifier (original)
   - client_id
   - redirect_uri

                                   10. Valida code_verifier contra code_challenge
                                   11. Marca AuthorizationCode como usado
                                   12. Gera Access Token (JWT) + Refresh Token
                                   13. Retorna os tokens

14. Armazena tokens
15. Usa Access Token nas requisições para suas APIs
```

---

## Fluxo: Renovação de Token (Refresh)

```
1. Access Token expira (padrão: 15 minutos)
2. App envia o Refresh Token para /bff/public/oauth/token/refresh
3. AuthPlatform valida:
   → Token existe no banco?
   → IsRevoked = false?
   → ExpiresAt ainda no futuro?
4. Se válido:
   → Revoga o Refresh Token atual (IsRevoked = true)
   → Gera novo Refresh Token
   → Atualiza ReplacedByToken no token antigo
   → Gera novo Access Token
   → Retorna os dois novos tokens
5. Se inválido:
   → Retorna 401
   → App redireciona para login
```

---

## Estrutura do JWT (Access Token)

```json
{
  "sub": "user-guid",
  "email": "usuario@exemplo.com",
  "name": "João Silva",
  "roles": ["Admin", "Editor"],
  "tenant_id": "tenant-guid",
  "client_id": "client-guid",
  "scope": "openid profile email roles",
  "jti": "token-unique-id",
  "iat": 1700000000,
  "exp": 1700000900
}
```

| Campo | Significado |
|---|---|
| `sub` | Subject — ID do usuário |
| `jti` | JWT ID — identificador único do token. Usado na revogação |
| `iat` | Issued At — quando foi emitido |
| `exp` | Expiration — quando expira (15 min após `iat`) |
| `tenant_id` | Identifica de qual tenant o usuário veio |
| `roles` | Roles do usuário nesse tenant |