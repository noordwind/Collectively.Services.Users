using System;
using System.Threading.Tasks;
using Collectively.Common;
using Collectively.Common.Commands;
using Collectively.Common.Domain;
using Collectively.Common.Services;
using Collectively.Services.Users.Services;

using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using RawRabbit;

namespace Collectively.Services.Users.Handlers
{
    public class ChangePasswordHandler : ICommandHandler<ChangePassword>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IPasswordService _passwordService;

        public ChangePasswordHandler(IHandler handler,
            IBusClient bus, 
            IPasswordService passwordService)
        {
            _handler = handler;
            _bus = bus;
            _passwordService = passwordService;
        }

        public async Task HandleAsync(ChangePassword command)
        {
            await _handler
                .Run(async () => await _passwordService.ChangeAsync(command.UserId, 
                    command.CurrentPassword, command.NewPassword))
                .OnSuccess(async () => await _bus.PublishAsync(
                    new PasswordChanged(command.Request.Id, command.UserId)))
                .OnCustomError(async ex => await _bus.PublishAsync(
                    new ChangePasswordRejected(command.Request.Id, command.UserId,
                        ex.Code, ex.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error when trying to change password.");
                    await _bus.PublishAsync(new ChangePasswordRejected(command.Request.Id, command.UserId,
                        OperationCodes.Error, "Error when trying to change password."));
                })
                .ExecuteAsync();
        }
    }
}