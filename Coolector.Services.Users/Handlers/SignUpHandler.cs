using System;
using System.Threading.Tasks;
using Coolector.Common.Commands;
using Coolector.Common.Commands.Users;
using Coolector.Common.Events.Users;
using Coolector.Services.Users.Domain;
using Coolector.Services.Users.Services;
using RawRabbit;

namespace Coolector.Services.Users.Handlers
{
    public class SignUpHandler : ICommandHandler<SignUp>
    {
        private readonly IBusClient _bus;
        private readonly IUserService _userService;

        public SignUpHandler(IBusClient bus,
            IUserService userService)
        {
            _bus = bus;
            _userService = userService;
        }

        public async Task HandleAsync(SignUp command)
        {
            var userId = Guid.NewGuid().ToString("N");
            await _userService.SignUpAsync(userId, command.Email,
                Roles.User, Providers.Coolector, command.Password);
            var user = await _userService.GetAsync(userId);
            await _bus.PublishAsync(new UserSignedUp(command.Request.Id, userId, user.Value.Email, user.Value.Name,
                string.Empty, user.Value.Role, user.Value.State, user.Value.Provider, user.Value.CreatedAt));
        }
    }
}