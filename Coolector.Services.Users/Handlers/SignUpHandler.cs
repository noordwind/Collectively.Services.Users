using System;
using System.Threading.Tasks;
using Coolector.Common.Commands;
using Coolector.Common.Services;
using Coolector.Services.Users.Domain;
using Coolector.Services.Users.Services;
using Coolector.Services.Users.Shared;
using Coolector.Services.Users.Shared.Commands;
using Coolector.Services.Users.Shared.Events;
using RawRabbit;

namespace Coolector.Services.Users.Handlers
{
    public class SignUpHandler : ICommandHandler<SignUp>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IUserService _userService;

        public SignUpHandler(IHandler handler, 
            IBusClient bus,
            IUserService userService)
        {
            _handler = handler;
            _bus = bus;
            _userService = userService;
        }

        public async Task HandleAsync(SignUp command)
        {
            var userId = Guid.NewGuid().ToString("N");
            await _handler
                .Run(async () => await _userService.SignUpAsync(userId, command.Email,
                    Roles.User, Providers.Coolector,
                    password: command.Password, name: command.Name))
                .OnSuccess(async () =>
                {
                    var user = await _userService.GetAsync(userId);
                    await _bus.PublishAsync(new SignedUp(command.Request.Id, userId, user.Value.Email,
                        user.Value.Name, string.Empty, user.Value.Role, user.Value.State,
                        user.Value.Provider, string.Empty, user.Value.CreatedAt));
                })
                .OnCustomError(async ex => await _bus.PublishAsync(new SignUpRejected(command.Request.Id,
                    null, ex.Code, ex.Message, command.Provider)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error occured while signing up a user");
                    await _bus.PublishAsync(new SignUpRejected(command.Request.Id,
                        null, OperationCodes.Error, ex.Message, command.Provider));
                })
                .ExecuteAsync();
        }
    }
}