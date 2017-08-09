namespace Collectively.Services.Users.Domain
{
    public static class Roles
    {
        public static string User => "user";
        public static string Associate => "associate";
        public static string Moderator => "moderator";
        public static string Administrator => "administrator";

        public static bool IsValid(string role)
        => role == User || role == Associate || 
           role == Moderator || role == Administrator;
    }
}