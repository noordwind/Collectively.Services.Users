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
    public class SetNewPasswordHandler : ICommandHandler<SetNewPassword>
    {
        private readonly IBusClient _bus;
        private readonly IPasswordService _passwordService;

        public SetNewPasswordHandler(IBusClient bus, IPasswordService passwordService)
        {
            _bus = bus;
            _passwordService = passwordService;
        }

        public async Task HandleAsync(SetNewPassword command)
        {
            try
            {
                await _passwordService.SetNewAsync(command.Email, command.Token, command.Password);
                await _bus.PublishAsync(new NewPasswordSet(command.Request.Id, command.Email));
            }
            catch (ServiceException exception)
            {
                await _bus.PublishAsync(new SetNewPasswordRejected(command.Request.Id,
                    OperationCodes.InvalidEmail, exception.Message, command.Email));
            }
            catch (Exception exception)
            {
                await _bus.PublishAsync(new SetNewPasswordRejected(command.Request.Id,
                    OperationCodes.InvalidEmail, "Error when trying to set new password.", command.Email));
            }
        }
    }
}