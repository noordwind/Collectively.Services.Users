using System.Threading.Tasks;
using Coolector.Common.Commands;
using Coolector.Common.Commands.Users;
using Coolector.Common.Events.Users;
using Coolector.Common.Services;
using Coolector.Services.Users.Services;
using RawRabbit;

namespace Coolector.Services.Users.Handlers
{
    public class SignOutHandler : ICommandHandler<SignOut>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;

        public SignOutHandler(IHandler handler, IBusClient bus,
            IUserService userService,
            IAuthenticationService authenticationService)
        {
            _handler = handler;
            _bus = bus;
            _userService = userService;
            _authenticationService = authenticationService;
        }

        public async Task HandleAsync(SignOut command)
        {
            await _handler
                .Run(async () => await _authenticationService.SignOutAsync(command.SessionId, command.UserId))
                .OnSuccess(async () => await _bus.PublishAsync(new UserSignedOut(command.Request.Id,
                    command.UserId, command.SessionId)))
                .OnCustomError(async ex => await _bus.PublishAsync(new SignOutRejected(command.Request.Id,
                    command.UserId, ex.Code, ex.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error("Error occured while signing out");
                    await _bus.PublishAsync(new SignOutRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message));
                })
                .ExecuteAsync();
        }
    }
}