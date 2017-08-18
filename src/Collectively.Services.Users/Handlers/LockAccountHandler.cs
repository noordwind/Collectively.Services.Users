using System.Threading.Tasks;
using Collectively.Messages.Commands;
using Collectively.Common.Services;
using Collectively.Services.Users.Services;
using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using RawRabbit;

namespace Collectively.Services.Users.Handlers
{
    public class LockAccountHandler : ICommandHandler<LockAccount>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IUserService _userService;

        public LockAccountHandler(IHandler handler, 
            IBusClient bus,
            IUserService userService)
        {
            _handler = handler;
            _bus = bus;
            _userService = userService;
        }

        public async Task HandleAsync(LockAccount command)
        {
            await _handler
                .Run(async () => await _userService.LockAsync(command.LockUserId))
                .OnSuccess(async () => await _bus.PublishAsync(new AccountLocked(command.Request.Id,
                    command.UserId, command.LockUserId)))
                .OnCustomError(async ex => await _bus.PublishAsync(new LockAccountRejected(command.Request.Id,
                    command.UserId, ex.Code, ex.Message, command.LockUserId)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error($"Error occured while locking an account for user: '{command.UserId}'.");
                    await _bus.PublishAsync(new LockAccountRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message, command.LockUserId));
                })
                .ExecuteAsync();
        }
    }
}