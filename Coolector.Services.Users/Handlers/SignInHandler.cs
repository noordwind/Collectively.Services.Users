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
    public class SignInHandler : ICommandHandler<SignIn>
    {
        private readonly IBusClient _bus;
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;

        public SignInHandler(IBusClient bus,
            IUserService userService,
            IAuthenticationService authenticationService)
        {
            _bus = bus;
            _userService = userService;
            _authenticationService = authenticationService;
        }

        public async Task HandleAsync(SignIn command)
        {
            var user = await _userService.GetByEmailAsync(command.Email, Providers.Coolector);
            if (user.HasNoValue)
            {
                await _bus.PublishAsync(new UserSignInRejected(command.Request.Id,
                    string.Empty, "User not found.", command.Provider));
            }

            try
            {
                await _authenticationService.SignInAsync(command.SessionId,
                    command.Email, command.Password, command.UserAgent);
                await _bus.PublishAsync(new UserSignedIn(command.Request.Id,
                    user.Value.UserId, user.Value.Email, user.Value.Name, user.Value.Provider));
            }
            catch (Exception ex)
            {
                await _bus.PublishAsync(new UserSignInRejected(command.Request.Id,
                    user.Value.UserId, "Invalid credentials", command.Provider));
            }
        }
    }
}