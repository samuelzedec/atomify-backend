# Módulo: Notifications

> Responsável por toda comunicação por e-mail disparada pela plataforma.

---

## Responsabilidades

- E-mail de boas-vindas após registro
- E-mail de confirmação de endereço
- Convite de usuário por e-mail
- Alerta de login em novo dispositivo ou IP suspeito
- Notificação de alteração de senha
- Código de MFA por e-mail (fallback quando sem app autenticador)

---

## Como funciona na arquitetura

O módulo de Notifications **nunca é chamado diretamente** pelos outros módulos. Ele **ouve eventos** internos e reage a eles.

```
Identity dispara evento → "UserRegistered"
                                ↓
                    Notifications ouve o evento
                                ↓
                    Busca template de e-mail
                                ↓
                    Envia via MailKit
```

Isso mantém o Identity sem nenhuma dependência de envio de e-mail. Se amanhã você quiser trocar MailKit por SendGrid, só muda o Notifications.

---

## Entidades

### EmailTemplate

Armazena os templates de e-mail de forma dinâmica, permitindo editar o conteúdo pelo painel admin sem redeploy.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador interno |
| `TenantId` | `Guid?` | Templates podem ser globais (nulo) ou específicos por tenant |
| `Type` | `enum` | Tipo do template: `Welcome`, `EmailConfirmation`, `PasswordChanged`, `LoginAlert`, `Invite`, `MfaCode` |
| `Subject` | `string` | Assunto do e-mail. Suporta variáveis: `"Bem-vindo, {{UserName}}!"` |
| `HtmlBody` | `string` | Corpo do e-mail em HTML. Suporta variáveis como `{{ConfirmationLink}}` |
| `IsActive` | `bool` | Permite desativar um tipo de notificação sem remover o template |
| `UpdatedAt` | `DateTime` | Última atualização do template |

> **Por que guardar templates no banco?**
> Se os templates fossem arquivos estáticos no projeto, qualquer ajuste de texto exigiria um novo deploy. No banco, o admin edita pelo painel e o próximo e-mail já sai com o novo texto.

---

### NotificationLog

Registro de cada e-mail enviado. Essencial para debugar problemas de entrega e auditar comunicações.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador do envio |
| `TenantId` | `Guid` | Tenant de origem |
| `UserId` | `Guid?` | Usuário destinatário. Nulo para e-mails de convite (usuário ainda não existe) |
| `ToEmail` | `string` | Endereço de destino |
| `Type` | `enum` | Tipo da notificação enviada |
| `Status` | `enum` | Resultado: `Sent`, `Failed`, `Bounced` |
| `ErrorMessage` | `string?` | Detalhes do erro em caso de falha no envio |
| `SentAt` | `DateTime` | Quando o e-mail foi processado e enviado |

---

### UserInvite

Representa um convite enviado para alguém que ainda não tem conta. O convite tem validade e ao ser aceito cria o usuário automaticamente.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador do convite |
| `TenantId` | `Guid` | Tenant para o qual o usuário está sendo convidado |
| `Email` | `string` | E-mail do convidado |
| `RoleId` | `Guid?` | Role que será atribuída automaticamente ao aceitar o convite |
| `Token` | `string` | Token único e secreto incluído no link do convite |
| `InvitedBy` | `Guid` | ID do admin que gerou o convite |
| `ExpiresAt` | `DateTime` | Validade do convite — padrão 48 horas |
| `AcceptedAt` | `DateTime?` | Quando o convite foi aceito. Nulo se ainda pendente |
| `IsRevoked` | `bool` | Admin pode cancelar um convite antes de ser aceito |
| `CreatedAt` | `DateTime` | Quando o convite foi gerado |

---

## Fluxo: Convite de usuário

```
1. Admin acessa o painel e convida "novo@usuario.com"
2. Sistema cria UserInvite com token único e ExpiresAt = agora + 48h
3. Notifications envia e-mail com link:
   https://authplatform.com/invite/accept?token=TOKEN
4. Convidado clica no link
5. Sistema valida: token existe, não expirou, não foi revogado
6. Convidado define sua senha
7. User é criado com IsEmailVerified = true e a Role do convite
8. UserInvite.AcceptedAt = agora
9. Convidado é redirecionado para login
```

---

## Eventos que o módulo ouve

| Evento | Ação |
|---|---|
| `UserRegistered` | Envia e-mail de boas-vindas + confirmação |
| `EmailConfirmationRequested` | Envia link de confirmação |
| `PasswordChanged` | Alerta o usuário sobre a alteração |
| `LoginFromNewIp` | Alerta de acesso de IP desconhecido |
| `UserInviteCreated` | Envia o e-mail de convite |
| `MfaCodeRequested` | Envia código por e-mail como fallback do TOTP |