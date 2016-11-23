using System;
using System.Threading.Tasks;
using Coolector.Common;
using Coolector.Common.Commands;
using Coolector.Common.Commands.Users;
using Coolector.Common.Domain;
using Coolector.Common.Events.Users;
using Coolector.Services.Users.Services;
using RawRabbit;

namespace Coolector.Services.Users.Handlers
{
    public class ChangePasswordHandler : ICommandHandler<ChangePassword>
    {
        private readonly IBusClient _bus;
        private readonly IPasswordService _passwordService;

        public ChangePasswordHandler(IBusClient bus, IPasswordService passwordService)
        {
            _bus = bus;
            _passwordService = passwordService;
        }

        public async Task HandleAsync(ChangePassword command)
        {
            try
            {
                await _passwordService.ChangeAsync(command.UserId, command.CurrentPassword,
                    command.NewPassword);
                await _bus.PublishAsync(new PasswordChanged(command.Request.Id, command.UserId));
            }
            catch (ServiceException exception)
            {
                await _bus.PublishAsync(new ChangePasswordRejected(command.Request.Id, command.UserId,
                    OperationCodes.InvalidPassword, exception.Message));
            }
            catch (Exception exception)
            {
                await _bus.PublishAsync(new ChangePasswordRejected(command.Request.Id, command.UserId,
                    OperationCodes.Error, "Error when trying to change password."));
            }
        }
    }
}