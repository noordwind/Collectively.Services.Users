using System.Threading.Tasks;
using Collectively.Messages.Commands;
using Collectively.Common.Services;
using Collectively.Services.Users.Services;
using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using RawRabbit;

namespace Collectively.Services.Users.Handlers
{
    public class SignOutHandler : ICommandHandler<SignOut>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IAuthenticationService _authenticationService;

        public SignOutHandler(IHandler handler, 
            IBusClient bus,
            IAuthenticationService authenticationService)
        {
            _handler = handler;
            _bus = bus;
            _authenticationService = authenticationService;
        }

        public async Task HandleAsync(SignOut command)
        {
            await _handler
                .Run(async () => await _authenticationService.SignOutAsync(command.SessionId, command.UserId))
                .OnSuccess(async () => await _bus.PublishAsync(new SignedOut(command.Request.Id,
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