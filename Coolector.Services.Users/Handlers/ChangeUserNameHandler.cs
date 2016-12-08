using System.Threading.Tasks;
using Coolector.Common.Commands;
using Coolector.Common.Services;
using Coolector.Services.Users.Services;
using Coolector.Services.Users.Shared;
using Coolector.Services.Users.Shared.Commands;
using Coolector.Services.Users.Shared.Events;
using RawRabbit;

namespace Coolector.Services.Users.Handlers
{
    public class ChangeUserNameHandler : ICommandHandler<ChangeUserName>
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

        public async Task HandleAsync(ChangeUserName command)
        {
            await _handler
                .Run(async () => await _userService.ChangeNameAsync(command.UserId, command.Name))
                .OnSuccess(async () =>
                {
                    var user = await _userService.GetAsync(command.UserId);
                    await _bus.PublishAsync(new UserNameChanged(command.Request.Id, 
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