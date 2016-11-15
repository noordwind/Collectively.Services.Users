using System.Threading.Tasks;
using Coolector.Common.Commands;
using Coolector.Common.Commands.Users;
using Coolector.Common.Events.Users;
using Coolector.Services.Users.Services;
using RawRabbit;

namespace Coolector.Services.Users.Handlers
{
    public class SignOutHandler : ICommandHandler<SignOut>
    {
        private readonly IBusClient _bus;
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;

        public SignOutHandler(IBusClient bus,
            IUserService userService,
            IAuthenticationService authenticationService)
        {
            _bus = bus;
            _userService = userService;
            _authenticationService = authenticationService;
        }

        public async Task HandleAsync(SignOut command)
        {
            var user = await _userService.GetAsync(command.UserId);
            if (user.HasNoValue)
                return;

            await _authenticationService.SignOutAsync(command.SessionId, command.UserId);
            await _bus.PublishAsync(new UserSignedOut(command.Request.Id,
                command.UserId, command.SessionId));
        }
    }
}