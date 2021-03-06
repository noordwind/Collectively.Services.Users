using System.Threading.Tasks;
using Collectively.Messages.Commands;
using Collectively.Common.Services;
using Collectively.Services.Users.Services;
using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using RawRabbit;

namespace Collectively.Services.Users.Handlers
{
    public class UnlockAccountHandler : ICommandHandler<UnlockAccount>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IUserService _userService;

        public UnlockAccountHandler(IHandler handler, 
            IBusClient bus,
            IUserService userService)
        {
            _handler = handler;
            _bus = bus;
            _userService = userService;
        }

        public async Task HandleAsync(UnlockAccount command)
        {
            await _handler
                .Run(async () => await _userService.UnlockAsync(command.UnlockUserId))
                .OnSuccess(async () => await _bus.PublishAsync(new AccountUnlocked(command.Request.Id,
                    command.UserId, command.UnlockUserId)))
                .OnCustomError(async ex => await _bus.PublishAsync(new UnlockAccountRejected(command.Request.Id,
                    command.UserId, ex.Code, ex.Message, command.UnlockUserId)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error($"Error occured while unlocking an account for user: '{command.UserId}'.");
                    await _bus.PublishAsync(new LockAccountRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message, command.UnlockUserId));
                })
                .ExecuteAsync();
        }
    }
}