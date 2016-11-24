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
    public class ResetPasswordHandler : ICommandHandler<ResetPassword>
    {
        private readonly IBusClient _bus;
        private readonly IPasswordService _passwordService;

        public ResetPasswordHandler(IBusClient bus, IPasswordService passwordService)
        {
            _bus = bus;
            _passwordService = passwordService;
        }

        public async Task HandleAsync(ResetPassword command)
        {
            try
            {
                await _passwordService.ResetAsync(command.Email);
                await _bus.PublishAsync(new ResetPasswordInitiated(command.Request.Id, command.Email));
            }
            catch (ServiceException exception)
            {
                await _bus.PublishAsync(new ResetPasswordRejected(command.Request.Id,
                    exception.Message, OperationCodes.InvalidEmail, command.Email));
            }
            catch (Exception exception)
            {
                await _bus.PublishAsync(new ResetPasswordRejected(command.Request.Id,
                    exception.Message, OperationCodes.Error, command.Email));
            }
        }
    }
}