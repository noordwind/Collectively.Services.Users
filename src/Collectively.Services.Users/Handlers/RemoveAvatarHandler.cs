using System.Threading.Tasks;
using Collectively.Messages.Commands;
using Collectively.Common.Services;
using Collectively.Services.Users.Services;
using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using RawRabbit;

namespace Collectively.Services.Users.Handlers
{
    public class RemoveAvatarHandler : ICommandHandler<RemoveAvatar>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IAvatarService _avatarService;

        public RemoveAvatarHandler(IHandler handler, IBusClient bus, 
            IAvatarService avatarService)
        {
            _handler = handler;
            _bus = bus;
            _avatarService = avatarService;
        }

        public async Task HandleAsync(RemoveAvatar command)
        {
            var avatarUrl = string.Empty;
            await _handler
                .Run(async () => await _avatarService.RemoveAsync(command.UserId))
                .OnSuccess(async () => await _bus.PublishAsync(new AvatarRemoved(command.Request.Id, 
                    command.UserId)))
                .OnCustomError(async ex => await _bus.PublishAsync(new RemoveAvatarRejected(command.Request.Id,
                        command.UserId, ex.Code, ex.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error occured while removing avatar.");
                    await _bus.PublishAsync(new RemoveAvatarRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message));
                })
                .ExecuteAsync();
        }
    }
}