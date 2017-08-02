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
    public class ActivateAccountHandler : ICommandHandler<ActivateAccount>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IUserService _userService;

        public ActivateAccountHandler(IHandler handler,
            IBusClient bus,
            IUserService userService)
        {
            _handler = handler;
            _bus = bus;
            _userService = userService;
        }

        public async Task HandleAsync(ActivateAccount command)
        {
            await _handler
                .Run(async () => await _userService
                    .ActivateAsync(command.Email, command.Token))
                .OnSuccess(async () =>
                    {
                        var user = await _userService.GetByEmailAsync(command.Email, Providers.Collectively);
                        await _bus
                            .PublishAsync(new AccountActivated(command.Request.Id, command.Email, user.Value.UserId));
                    })
                .OnCustomError(async ex => await _bus
                    .PublishAsync(new ActivateAccountRejected(command.Request.Id,
                        command.Email, ex.Code, ex.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error when activating account.");
                    await _bus.PublishAsync(new ActivateAccountRejected(command.Request.Id,
                        command.Email, OperationCodes.Error, "error when activating account"));
                })
                .ExecuteAsync();
        }
    }
}
