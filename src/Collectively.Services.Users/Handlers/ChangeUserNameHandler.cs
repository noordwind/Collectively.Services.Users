using System.Threading.Tasks;
using Collectively.Messages.Commands;
using Collectively.Common.Services;
using Collectively.Services.Users.Services;

using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using RawRabbit;

namespace Collectively.Services.Users.Handlers
{
    public class ChangeUserNameHandler : ICommandHandler<ChangeUsername>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IUserService _userService;

        public ChangeUserNameHandler(IHandler handler, 
            IBusClient bus, IUserService userService)
        {
            _handler = handler;
            _bus = bus;
            _userService = userService;
        }

        public async Task HandleAsync(ChangeUsername command)
        {
            await _handler
                .Run(async () => await _userService.ChangeNameAsync(command.UserId, command.Name))
                .OnSuccess(async () =>
                {
                    var user = await _userService.GetAsync(command.UserId);
                    await _bus.PublishAsync(new UsernameChanged(command.Request.Id, 
                        command.UserId, command.Name, user.Value.State));
                })
                .OnCustomError(async ex => await _bus.PublishAsync(new ChangeUsernameRejected(command.Request.Id,
                    command.UserId, ex.Code, ex.Message, command.Name)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error occured while changing username");
                    await _bus.PublishAsync(new ChangeUsernameRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message, command.Name));
                })
                .ExecuteAsync();
        }
    }
}