using AutoMapper;
using Coolector.Common.Dto;
using Coolector.Services.Users.Domain;
using Coolector.Services.Users.Queries;
using Coolector.Services.Users.Services;
using Coolector.Services.Users.Shared.Dto;

namespace Coolector.Services.Users.Modules
{
    public class UserModule : ModuleBase
    {
        public UserModule(IUserService userService, IMapper mapper) : base(mapper, "users")
        {
            Get("", async args => await FetchCollection<BrowseUsers, User>
                (async x => await userService.BrowseAsync(x))
                .MapTo<UserDto>()
                .HandleAsync());

            Get("{id}", async args => await Fetch<GetUser, User>
                (async x => await userService.GetAsync(x.Id))
                .MapTo<UserDto>()
                .HandleAsync());

            Get("{name}/account", async args => await Fetch<GetUserByName, User>
                (async x => await userService.GetByNameAsync(x.Name))
                .MapTo<UserDto>()
                .HandleAsync());

            Get("{name}/available", async args => await Fetch<GetNameAvailability, dynamic>
                (async x => await userService.IsNameAvailableAsync(x.Name))
                .MapTo<AvailableResourceDto>()
                .HandleAsync());
        }
    }
}