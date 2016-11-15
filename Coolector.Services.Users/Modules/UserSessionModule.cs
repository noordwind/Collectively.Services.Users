using Coolector.Services.Users.Domain;
using Coolector.Services.Users.Queries;
using Coolector.Services.Users.Services;

namespace Coolector.Services.Users.Modules
{
    public class UserSessionModule : ModuleBase
    {
        public UserSessionModule(IAuthenticationService authenticationService) : base("user-sessions")
        {
            Get("{id}", async args => await Fetch<GetUserSession, UserSession>
                (async x => await authenticationService.GetSessionAsync(x.Id)).HandleAsync());
        }
    }
}