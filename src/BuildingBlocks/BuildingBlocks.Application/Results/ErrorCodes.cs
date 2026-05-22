namespace BuildingBlocks.Application.Results;

public static class ErrorCodes
{
    public static class Auth
    {
        public const string InvalidCredentials = "invalid_credentials";
        public const string EmailNotConfirmed = "email_not_confirmed";
        public const string AccountLocked = "account_locked";
        public const string AccountDisabled = "account_disabled";
        public const string MfaRequired = "mfa_required";
    }

    public static class Users
    {
        public const string NotFound = "user_not_found";
        public const string EmailAlreadyTaken = "email_already_taken";
        public const string PasswordTooWeak = "password_too_weak";
    }

    public static class Realms
    {
        public const string NotFound = "realm_not_found";
        public const string AlreadyExists = "realm_already_exists";
    }

    public static class Roles
    {
        public const string NotFound = "role_not_found";
        public const string AlreadyAssigned = "role_already_assigned";
    }
}