using System;
using System.Threading.Tasks;
using Collectively.Messages.Commands;
using Collectively.Common.Services;
using Collectively.Messages.Commands.Mailing;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Services;
using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using Collectively.Services.Users.Settings;
using RawRabbit;

namespace Collectively.Services.Users.Handlers
{
    public class SignUpHandler : ICommandHandler<SignUp>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IUserService _userService;
        private readonly IOneTimeSecuredOperationService _oneTimeSecuredOperationService;
        private readonly IResourceFactory _resourceFactory;
        private readonly AppSettings _settings;

        public SignUpHandler(IHandler handler, 
            IBusClient bus,
            IUserService userService,
            IOneTimeSecuredOperationService oneTimeSecuredOperationService,
            IResourceFactory resourceFactory,
            AppSettings settings)
        {
            _handler = handler;
            _bus = bus;
            _userService = userService;
            _oneTimeSecuredOperationService = oneTimeSecuredOperationService;
            _resourceFactory = resourceFactory;
            _settings = settings;
        }

        public async Task HandleAsync(SignUp command)
        {
            var userId = Guid.NewGuid().ToString("N");
            await _handler
                .Run(async () => await _userService.SignUpAsync(userId, command.Email,
                    Roles.User, Providers.Collectively,
                    password: command.Password, name: command.Name,
                    culture: command.Request.Culture,
                    activate: false))
                .OnSuccess(async () =>
                {
                    var user = await _userService.GetAsync(userId);
                    var resource = _resourceFactory.Resolve<SignedUp>(userId);
                    await _bus.PublishAsync(new SignedUp(command.Request.Id, resource, 
                        userId, user.Value.Provider));
                    await PublishSendActivationEmailMessageCommandAsync(user.Value, command.Request);
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

        private async Task PublishSendActivationEmailMessageCommandAsync(User user, Request commandRequest)
        {
            var operationId = Guid.NewGuid();
            await _oneTimeSecuredOperationService.CreateAsync(operationId, OneTimeSecuredOperations.ActivateAccount,
                user.Email, DateTime.UtcNow.AddDays(7));
            var operation = await _oneTimeSecuredOperationService.GetAsync(operationId);
            var command = new SendActivateAccountEmailMessage
            {
                Email = user.Email,
                Username = user.Name,
                Token = operation.Value.Token,
                Endpoint = _settings.ActivateAccountUrl,
                Request = Request.From<SendActivateAccountEmailMessage>(commandRequest)
            };
            await _bus.PublishAsync(command);
        }
    }
}