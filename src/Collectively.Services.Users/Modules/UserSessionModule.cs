using AutoMapper;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Queries;
using Collectively.Services.Users.Services;
using Collectively.Services.Users.Dto;

namespace Collectively.Services.Users.Modules
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