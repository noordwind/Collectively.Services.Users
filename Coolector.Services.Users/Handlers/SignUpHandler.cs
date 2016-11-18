using System;
using System.Threading.Tasks;
using Coolector.Common.Commands;
using Coolector.Common.Commands.Users;
using Coolector.Common.Domain;
using Coolector.Common.Events.Users;
using Coolector.Services.Users.Domain;
using Coolector.Services.Users.Services;
using NLog;
using RawRabbit;

namespace Coolector.Services.Users.Handlers
{
    public class SignUpHandler : ICommandHandler<SignUp>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IBusClient _bus;
        private readonly IUserService _userService;

        public SignUpHandler(IBusClient bus,
            IUserService userService)
        {
            _bus = bus;
            _userService = userService;
        }

        public async Task HandleAsync(SignUp command)
        {
            try
            {
                var userId = Guid.NewGuid().ToString("N");
                await _userService.SignUpAsync(userId, command.Email,
                    Roles.User, Providers.Coolector,
                    password: command.Password, name: command.Name);
                var user = await _userService.GetAsync(userId);
                await _bus.PublishAsync(new UserSignedUp(command.Request.Id, userId, user.Value.Email,
                    user.Value.Name, string.Empty, user.Value.Role, user.Value.State,
                    user.Value.Provider, string.Empty, user.Value.CreatedAt));
            }
            catch (ServiceException ex)
            {
                Logger.Error(ex);
                await _bus.PublishAsync(new UserSignUpRejected(command.Request.Id,
                    null, ex.Message, command.Provider));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                await _bus.PublishAsync(new UserSignUpRejected(command.Request.Id,
                    null, "Invalid credentials", command.Provider));
            }
        }
    }
}