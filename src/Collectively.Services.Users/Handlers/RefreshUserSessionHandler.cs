using System;
using System.Threading.Tasks;
using Collectively.Common.Services;
using Collectively.Messages.Commands;
using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Services;
using RawRabbit;

namespace Collectively.Services.Users.Handlers
{
    public class RefreshUserSessionHandler : ICommandHandler<RefreshUserSession>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IAuthenticationService _authenticationService;

        public RefreshUserSessionHandler(IHandler handler,
            IBusClient bus,
            IAuthenticationService authenticationService)
        {
            _handler = handler;
            _bus = bus;
            _authenticationService = authenticationService;
        }

        public async Task HandleAsync(RefreshUserSession command)
            => await _handler
                .Run(async () => await _authenticationService.RefreshSessionAsync(command.SessionId, 
                    command.NewSessionId, command.Key))
                .OnError((ex, logger) => logger.Error(ex, "Error when refreshing user session."))
                .ExecuteAsync();
    }
}
