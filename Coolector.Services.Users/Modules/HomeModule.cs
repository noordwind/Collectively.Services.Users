namespace Coolector.Services.Users.Modules
{
    public class HomeModule : ModuleBase
    {
        public HomeModule() : base(requireAuthentication: false)
        {
            Get("", args => "Welcome to the Coolector.Services.Users API!");
        }
    }
}