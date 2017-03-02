namespace Collectively.Services.Users
{
    public static class OperationCodes
    {
        public static string Success => "success";
        public static string UserNotFound => "user_not_found";
        public static string UserIdInUse => "user_id_in_use";
        public static string InactiveUser => "inactive_user";
        public static string SessionNotFound => "session_not_found";
        public static string InvalidSessionKey => "invalid_session_key";
        public static string SessionExpired => "session_expired";
        public static string InvalidCredentials => "invalid_credentials";
        public static string EmailInUse => "email_in_use";
        public static string NameInUse => "name_in_use";
        public static string NameAlreadySet => "name_already_set";
        public static string InvalidAccountType => "invalid_account_type";
        public static string InvalidName => "invalid_name";
        public static string InvalidEmail => "invalid_email";
        public static string InvalidPassword => "invalid_password";
        public static string InvalidFile => "invalid_file";
        public static string InvalidCurrentPassword => "invalid_current_password";
        public static string EmailNotFound => "email_not_found";
        public static string InvalidPasswordResetToken => "invalid_password_reset_token";
        public static string FileTooBig => "file_too_big";
        public static string TextTooLong => "text_too_long";
        public static string OperationNotFound => "operation_not_found";
        public static string InvalidSecuredOperation => "invalid_secured_operation";
        public static string Error => "error";
        public static string InvalidUser => "invalid_user";
    }
}