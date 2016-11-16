using System;
using System.Threading.Tasks;
using Coolector.Common.Commands;
using Coolector.Common.Commands.Users;
using Coolector.Common.Domain;
using Coolector.Common.Events.Users;
using Coolector.Services.Users.Services;
using NLog;
using RawRabbit;

namespace Coolector.Services.Users.Handlers
{
    public class SignInHandler : ICommandHandler<SignIn>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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
            try
            {
                await _authenticationService.SignInAsync(command.SessionId,
                    command.Email, command.Password, command.IpAddress, command.UserAgent);
                var user = await _userService.GetByEmailAsync(command.Email, command.Provider);
                await _bus.PublishAsync(new UserSignedIn(command.Request.Id,
                    user.Value.UserId, user.Value.Email, user.Value.Name, user.Value.Provider));
            }
            catch (ServiceException ex)
            {
                Logger.Error(ex);
                await _bus.PublishAsync(new UserSignInRejected(command.Request.Id,
                    null, ex.Message, command.Provider));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                await _bus.PublishAsync(new UserSignInRejected(command.Request.Id,
                    null, "Invalid credentials", command.Provider));
            }
        }
    }
}