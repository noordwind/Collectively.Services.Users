using System;
using System.Threading.Tasks;
using Coolector.Common;
using Coolector.Common.Commands;
using Coolector.Common.Commands.Mailing;
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
        private readonly IOneTimeSecuredOperationService _oneTimeSecuredOperationService;

        public ResetPasswordHandler(IBusClient bus, IPasswordService passwordService,
            IOneTimeSecuredOperationService oneTimeSecuredOperationService)
        {
            _bus = bus;
            _passwordService = passwordService;
            _oneTimeSecuredOperationService = oneTimeSecuredOperationService;
        }

        public async Task HandleAsync(ResetPassword command)
        {
            try
            {
                var operationId = Guid.NewGuid();
                await _passwordService.ResetAsync(operationId, command.Email);
                var operation = await _oneTimeSecuredOperationService.GetAsync(operationId);
                await _bus.PublishAsync(new SendResetPasswordEmailMessage
                {
                    Email = command.Email,
                    Endpoint = command.Endpoint,
                    Token = operation.Value.Token,
                    Request = Request.From<SendResetPasswordEmailMessage>(command.Request)
                });
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