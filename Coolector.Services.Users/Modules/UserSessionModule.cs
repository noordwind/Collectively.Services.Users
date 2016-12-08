using AutoMapper;
using Coolector.Services.Users.Domain;
using Coolector.Services.Users.Queries;
using Coolector.Services.Users.Services;
using Coolector.Services.Users.Shared.Dto;

namespace Coolector.Services.Users.Modules
{
    public class UserSessionModule : ModuleBase
    {
        public UserSessionModule(IAuthenticationService authenticationService, IMapper mapper) 
            : base(mapper, "user-sessions")
        {
            Get("{id}", async args => await Fetch<GetUserSession, UserSession>
                (async x => await authenticationService.GetSessionAsync(x.Id))
                .MapTo<UserSessionDto>()
                .HandleAsync());
        }
    }
}