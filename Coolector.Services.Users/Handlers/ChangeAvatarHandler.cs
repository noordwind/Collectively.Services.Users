using System.Threading.Tasks;
using Coolector.Common.Commands;
using Coolector.Common.Commands.Users;
using Coolector.Common.Events.Users;
using Coolector.Common.Services;
using Coolector.Services.Users.Services;
using RawRabbit;

namespace Coolector.Services.Users.Handlers
{
    public class ChangeAvatarHandler : ICommandHandler<ChangeAvatar>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IUserService _userService;

        public ChangeAvatarHandler(IHandler handler, IBusClient bus, IUserService userService)
        {
            _handler = handler;
            _bus = bus;
            _userService = userService;
        }

        public async Task HandleAsync(ChangeAvatar command)
        {
            await _handler
                .Run(async () => await _userService.ChangeAvatarAsync(command.UserId, command.PictureUrl))
                .OnSuccess(async () => await _bus.PublishAsync(new AvatarChanged(command.Request.Id, 
                    command.UserId, command.PictureUrl)))
                .OnCustomError(async ex => await _bus.PublishAsync(new ChangeAvatarRejected(command.Request.Id,
                        command.UserId, ex.Code, ex.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error occured while changing avatar");
                    await _bus.PublishAsync(new ChangeAvatarRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message));
                })
                .ExecuteAsync();
        }
    }
}