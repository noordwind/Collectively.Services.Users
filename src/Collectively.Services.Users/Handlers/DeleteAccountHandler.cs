using System.Threading.Tasks;
using Collectively.Messages.Commands;
using Collectively.Common.Services;
using Collectively.Services.Users.Services;
using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using RawRabbit;

namespace Collectively.Services.Users.Handlers
{
    public class DeleteAccountHandler : ICommandHandler<DeleteAccount>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IUserService _userService;

        public DeleteAccountHandler(IHandler handler, 
            IBusClient bus,
            IUserService userService)
        {
            _handler = handler;
            _bus = bus;
            _userService = userService;
        }

        public async Task HandleAsync(DeleteAccount command)
        {
            await _handler
                .Run(async () => await _userService.DeleteAsync(command.UserId, command.Soft))
                .OnSuccess(async () => await _bus.PublishAsync(new AccountDeleted(command.Request.Id,
                    command.UserId, command.Soft)))
                .OnCustomError(async ex => await _bus.PublishAsync(new DeleteAccountRejected(command.Request.Id,
                    command.UserId, ex.Code, ex.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error($"Error occured while deleting account for user: '{command.UserId}', " +
                                 $"soft: {command.Soft}.");
                    await _bus.PublishAsync(new DeleteAccountRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message));
                })
                .ExecuteAsync();
        }
    }
}