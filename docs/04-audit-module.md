# Módulo: Audit

> Responsável por registrar todas as ações sensíveis que acontecem na plataforma, criando uma trilha de auditoria completa e imutável.

---

## Responsabilidades

- Registrar logins bem-sucedidos e falhas de autenticação
- Registrar criação, edição e exclusão de usuários
- Registrar alterações de permissões e roles
- Registrar ações de administradores no painel
- Expor histórico de auditoria filtrado por tenant, usuário ou ação

---

## Por que auditoria é um módulo separado?

Auditoria tem características únicas que justificam o isolamento:

- **Registros são imutáveis** — ninguém deve poder editar ou apagar um log de auditoria
- **Volume alto** — toda ação gera um registro, o que impacta a tabela se misturada com entidades de negócio
- **Consulta independente** — o painel admin consulta auditoria diretamente, sem passar pelo Identity ou OAuth
- **Evolução independente** — você pode trocar a estratégia de armazenamento (ex: mover para Elasticsearch) sem tocar nos outros módulos

---

## Entidades

### AuditLog

Registro imutável de uma ação sensível. Uma vez criado, nunca é editado — apenas inserido e consultado.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador único do registro |
| `TenantId` | `Guid` | Tenant onde a ação aconteceu |
| `UserId` | `Guid?` | Usuário que executou a ação. Nulo para ações de sistema ou pré-autenticação |
| `ActorEmail` | `string?` | E-mail do ator no momento da ação. Guardado diretamente para não perder o histórico se o e-mail mudar depois |
| `Action` | `enum` | Tipo da ação realizada (ver tabela abaixo) |
| `EntityType` | `string?` | Entidade afetada pela ação: `"User"`, `"OAuthClient"`, `"Role"` |
| `EntityId` | `string?` | ID da entidade afetada. String para suportar diferentes tipos de ID |
| `OldValues` | `string?` | Estado anterior em JSON. Preenchido em ações de edição para saber o que mudou |
| `NewValues` | `string?` | Estado novo em JSON. Preenchido em ações de criação e edição |
| `IpAddress` | `string` | IP de origem da requisição |
| `UserAgent` | `string?` | Navegador/cliente usado na requisição |
| `CreatedAt` | `DateTime` | Momento exato da ação — sempre em UTC |

---

### Tipos de Ação (enum Action)

| Valor | Descrição |
|---|---|
| `LoginSuccess` | Login bem-sucedido |
| `LoginFailed` | Tentativa de login com credenciais inválidas |
| `LoginBlocked` | Tentativa durante bloqueio de conta |
| `Logout` | Logout explícito |
| `TokenRefreshed` | Refresh token usado para renovar sessão |
| `TokenRevoked` | Token revogado explicitamente |
| `MfaEnabled` | Usuário ativou MFA |
| `MfaDisabled` | Usuário desativou MFA |
| `MfaFailed` | Código TOTP inválido informado |
| `PasswordChanged` | Senha alterada pelo próprio usuário |
| `PasswordResetRequested` | Solicitação de reset de senha |
| `UserCreated` | Novo usuário registrado |
| `UserUpdated` | Dados do usuário editados |
| `UserDeactivated` | Usuário desativado pelo admin |
| `UserLocked` | Conta bloqueada por tentativas falhas |
| `UserUnlocked` | Bloqueio removido pelo admin |
| `RoleAssigned` | Role atribuída a um usuário |
| `RoleRemoved` | Role removida de um usuário |
| `ClientCreated` | Nova aplicação OAuth registrada |
| `ClientDeactivated` | Aplicação OAuth desativada |
| `InviteSent` | Convite de usuário enviado |
| `InviteAccepted` | Convite aceito e usuário criado |
| `InviteRevoked` | Convite cancelado pelo admin |

---

## Como o módulo recebe as ações

Assim como Notifications, o Audit **ouve eventos** disparados pelos outros módulos. Ele nunca é chamado diretamente.

```
Identity: usuário faz login → dispara evento "LoginSucceeded"
                                        ↓
                           Audit ouve o evento
                                        ↓
                           Cria AuditLog imutável no PostgreSQL
```

---

## Fluxo: Consulta de auditoria no painel admin

```
1. Admin acessa "Logs de Auditoria" no painel Angular
2. Aplica filtros: tenant, usuário, tipo de ação, período
3. Angular chama GET /bff/admin/tenants/{id}/audit?action=LoginFailed&from=...
4. BFF Admin repassa a consulta para o módulo Audit
5. Audit consulta AuditLog com os filtros
6. BFF Admin formata e retorna lista paginada
7. Admin visualiza: quem fez o quê, quando e de qual IP
```

---

## Exemplo de registro

Para a ação de um admin atribuindo uma role a um usuário:

```json
{
  "id": "a1b2c3...",
  "tenantId": "tenant-guid",
  "userId": "admin-guid",
  "actorEmail": "admin@empresa.com",
  "action": "RoleAssigned",
  "entityType": "User",
  "entityId": "user-guid",
  "oldValues": null,
  "newValues": { "roleId": "role-guid", "roleName": "Editor" },
  "ipAddress": "189.100.200.50",
  "userAgent": "Mozilla/5.0 ...",
  "createdAt": "2024-11-15T14:32:00Z"
}
```

---

## Boas práticas aplicadas

- Registros nunca são deletados — apenas consultados
- `ActorEmail` é salvo diretamente para preservar histórico mesmo se o e-mail mudar
- `OldValues` e `NewValues` em JSON permitem rastrear exatamente o que mudou
- Todos os timestamps são UTC para evitar problemas de fuso horário
- Índices no banco em `TenantId + CreatedAt` para consultas rápidas por período