namespace Collectively.Services.Users.Domain
{
    public static class States
    {
        public static string Inactive => "inactive";
        public static string Incomplete => "incomplete";
        public static string Unconfirmed => "unconfirmed";
        public static string Active => "active";
        public static string Locked => "locked";
        public static string Deleted => "deleted";
    }
}