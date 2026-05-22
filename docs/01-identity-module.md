# Módulo: Identity

> Responsável por tudo relacionado ao ciclo de vida do usuário e autenticação.

---

## Responsabilidades

- Registro e login de usuários
- Autenticação com senha local e login social (Google, GitHub)
- MFA com TOTP (Google Authenticator / Authy)
- Controle de sessão via Refresh Token
- Bloqueio de conta após tentativas falhas
- Gerenciamento de roles do sistema e roles customizadas

---

## Modelo de Roles

Existem dois tipos de roles completamente separados:

### Roles do Sistema
Fixas, criadas no seed, protegidas — não podem ser editadas nem deletadas.

| Role | Onde existe | O que pode |
|---|---|---|
| `SuperAdmin` | Só no TenantPrincipal | Acesso global à plataforma. Navega entre todos os tenants. Único com acesso ao painel admin do Atomify |
| `TenantAdmin` | Em qualquer tenant | Gerencia usuários e roles customizadas do próprio tenant |
| `User` | Em qualquer tenant | Role padrão atribuída automaticamente a todo novo usuário |

### Roles Customizadas
Criadas pelo TenantAdmin dentro do próprio tenant. Totalmente livres — o tenant define os nomes e significados.

```
Tenant: Meu Portfólio
└── Roles customizadas: Editor, Reviewer

Tenant: App de Finanças
└── Roles customizadas: Contador, Gerente, Auditor
```

> Roles customizadas são isoladas por tenant. `Editor` no Meu Portfólio não tem nenhuma relação com `Editor` na App de Finanças.

---

## Como as roles se combinam no User

Todo usuário tem **obrigatoriamente uma SystemRole** e opcionalmente **zero ou mais roles customizadas**.

```
User: João (Tenant: Meu Portfólio)
├── SystemRole: User              ← atribuída automaticamente no registro
└── CustomRoles: Editor, Reviewer ← atribuídas pelo TenantAdmin

User: você (TenantPrincipal)
├── SystemRole: SuperAdmin        ← atribuída no seed
└── CustomRoles: (nenhuma)
```

No JWT isso se traduz assim:

```json
{
  "sub": "user-guid",
  "tenant_id": "tenant-guid",
  "system_role": "User",
  "roles": ["Editor", "Reviewer"]
}
```

```json
{
  "sub": "superadmin-guid",
  "tenant_id": "principal-tenant-guid",
  "system_role": "SuperAdmin",
  "roles": []
}
```

---

## Regras de atribuição de roles

| Quem | Pode atribuir |
|---|---|
| Sistema (seed) | `SuperAdmin` ao primeiro usuário no boot |
| SuperAdmin | `TenantAdmin` a qualquer usuário de qualquer tenant |
| TenantAdmin | `User` + roles customizadas do próprio tenant |
| User | Nada |

---

## Entidades

### User

Entidade central do sistema. Representa qualquer pessoa que tem acesso a uma aplicação cadastrada no Atomify.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador único global. Usado como `sub` (subject) dentro do JWT |
| `TenantId` | `Guid` | Isola o usuário dentro de uma aplicação. O mesmo e-mail pode existir em tenants diferentes |
| `Email` | `string` | Identificador de login principal. Sempre salvo em lowercase para evitar duplicatas |
| `PasswordHash` | `string?` | Hash da senha com Argon2. É nulo para usuários que só usam login social |
| `FirstName` | `string?` | Nome do usuário. Opcional no cadastro |
| `LastName` | `string?` | Sobrenome do usuário. Opcional no cadastro |
| `IsEmailVerified` | `bool` | Indica se o usuário confirmou o e-mail. Usuários não verificados têm acesso limitado |
| `IsActive` | `bool` | Controle manual pelo admin. Desativar não exclui o usuário nem seus dados |
| `IsLocked` | `bool` | Bloqueio automático após N tentativas de login falhas consecutivas |
| `FailedLoginAttempts` | `int` | Contador de tentativas falhas. Resetado após login bem-sucedido |
| `LockedUntil` | `DateTime?` | Define até quando o bloqueio é válido. Nulo significa sem bloqueio ativo |
| `MfaEnabled` | `bool` | Indica se o usuário ativou autenticação de dois fatores |
| `MfaSecret` | `string?` | Segredo TOTP usado para gerar os códigos de 6 dígitos. Armazenado criptografado |
| `SystemRole` | `enum` | Role do sistema obrigatória: `SuperAdmin`, `TenantAdmin` ou `User` |
| `IsProtected` | `bool` | Usuários protegidos não podem ser apagados, desativados ou ter a SystemRole alterada. Usado pelo SuperAdmin |
| `CreatedAt` | `DateTime` | Data de criação da conta |
| `LastLoginAt` | `DateTime?` | Data do último login bem-sucedido. Nulo se nunca logou |

**Relacionamentos:**
- Um `User` pertence a um `Tenant`
- Um `User` pode ter vários `ExternalLogin` (Google, GitHub)
- Um `User` pode ter vários `RefreshToken` ativos
- Um `User` pode ter vários `UserCustomRole` (roles customizadas do tenant)

---

### ExternalLogin

Vincula um usuário a um provedor de login social. Separada do `User` porque um mesmo usuário pode conectar Google E GitHub na mesma conta.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador interno do vínculo |
| `UserId` | `Guid` | FK para o usuário dono desse vínculo |
| `Provider` | `string` | Nome do provedor: `"Google"` ou `"GitHub"` |
| `ProviderUserId` | `string` | ID único do usuário dentro do provedor. Usado para identificá-lo no callback OAuth |
| `ProviderEmail` | `string?` | E-mail retornado pelo provedor. Pode ser diferente do e-mail principal do usuário |
| `LinkedAt` | `DateTime` | Quando esse provedor foi vinculado à conta |

> **Por que separar do User?**
> Se o usuário pudesse ter apenas um provedor, bastaria colocar `Provider` e `ProviderUserId` diretamente no `User`. Como ele pode ter múltiplos, criamos uma tabela própria com relação 1-N.

---

### RefreshToken

Representa um token de sessão de longa duração. Permite que o usuário continue logado sem redigitar a senha.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador do token |
| `UserId` | `Guid` | FK para o usuário dono da sessão |
| `Token` | `string` | Valor do token — string aleatória e única gerada com `RandomNumberGenerator` |
| `ExpiresAt` | `DateTime` | Data de expiração. Após esse momento o token é inválido mesmo que não tenha sido revogado |
| `IsRevoked` | `bool` | Revogação manual — logout, troca de senha ou suspeita de comprometimento |
| `ReplacedByToken` | `string?` | Quando o token é rotacionado, guarda qual token o substituiu. Permite rastrear a cadeia de rotação |
| `CreatedAt` | `DateTime` | Quando a sessão foi iniciada |
| `CreatedByIp` | `string` | IP de origem da sessão. Usado para detectar uso suspeito em IPs diferentes |
| `RevokedAt` | `DateTime?` | Quando foi revogado. Nulo se ainda está ativo |

> **O que é rotação de Refresh Token?**
> A cada uso do refresh token, o sistema o invalida e gera um novo. Isso garante que se alguém roubar um token, ele só funcionará uma vez antes de ser detectado.

---

### CustomRole

Define uma role customizada dentro de um tenant. Criada pelo TenantAdmin e totalmente livre em nome e significado.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador da role |
| `TenantId` | `Guid` | Roles customizadas são isoladas por tenant. `Editor` no tenant A não é o mesmo que `Editor` no tenant B |
| `Name` | `string` | Nome da role: `"Editor"`, `"Reviewer"`, `"Contador"` — livre |
| `Description` | `string?` | Descrição legível do que esse papel pode fazer dentro da app |
| `IsProtected` | `bool` | Roles protegidas não podem ser deletadas. Usado para roles críticas do tenant |
| `CreatedAt` | `DateTime` | Data de criação |

---

### UserCustomRole

Tabela de associação entre `User` e `CustomRole`. Um usuário pode ter múltiplas roles customizadas.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `UserId` | `Guid` | FK para o usuário |
| `RoleId` | `Guid` | FK para a role customizada |
| `AssignedAt` | `DateTime` | Quando a role foi atribuída |
| `AssignedBy` | `Guid` | ID do TenantAdmin que fez a atribuição. Importante para auditoria |

---

## Fluxo: Registro com senha local

```
1. Usuário envia e-mail + senha
2. Sistema verifica se e-mail já existe no tenant
3. Cria User com PasswordHash (Argon2)
4. SystemRole = User (atribuída automaticamente)
5. IsEmailVerified = false
6. Dispara evento → Notifications envia e-mail de confirmação
7. Usuário clica no link → IsEmailVerified = true
8. Usuário pode fazer login
```

---

## Fluxo: Login social (Google / GitHub)

```
1. Usuário clica em "Entrar com Google"
2. Redireciona para Google com Client ID do Atomify
3. Google autentica e redireciona de volta com um "code"
4. Atomify troca o "code" por dados do usuário na API do Google
5. Verifica se já existe ExternalLogin com esse ProviderUserId
   → Existe: faz login no User vinculado
   → Não existe: cria User + ExternalLogin (ou vincula a conta existente pelo e-mail)
6. SystemRole = User se for novo usuário
7. Gera Access Token + Refresh Token e retorna para a app
```

---

## Fluxo: MFA

```
1. Usuário ativa MFA no perfil
2. Sistema gera MfaSecret e exibe QR Code
3. Usuário escaneia com Google Authenticator
4. Usuário confirma com o primeiro código de 6 dígitos
5. MfaEnabled = true

No login:
1. Usuário entra com e-mail + senha → login válido
2. Sistema detecta MfaEnabled = true
3. Retorna status "mfa_required" em vez do token
4. Usuário envia o código TOTP de 6 dígitos
5. Sistema valida o código contra o MfaSecret
6. Gera Access Token + Refresh Token normalmente
```

---

## Fluxo: Bloqueio de conta

```
1. Usuário erra a senha
2. FailedLoginAttempts++
3. Se FailedLoginAttempts >= 5:
   → IsLocked = true
   → LockedUntil = agora + 15 minutos
4. Tentativas durante o bloqueio retornam erro sem consultar a senha
5. Após LockedUntil:
   → IsLocked = false automaticamente na próxima tentativa
   → FailedLoginAttempts = 0
6. Login bem-sucedido sempre reseta FailedLoginAttempts para 0
```

---

## Fluxo: Navegação entre tenants (SuperAdmin)

```
1. SuperAdmin loga com suas credenciais
2. JWT gerado com system_role = "SuperAdmin" e tenant_id = TenantPrincipal
3. Acessa o painel admin do Atomify
4. Seleciona qualquer tenant na lista
5. BFF Admin verifica: system_role == SuperAdmin?
   → Sim: usa o tenantId da rota — acesso liberado
   → Não: força o tenant_id do próprio token — acesso restrito
6. SuperAdmin vê usuários, roles, audit log e configurações do tenant selecionado
```