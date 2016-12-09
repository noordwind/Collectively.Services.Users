using System.Threading.Tasks;
using Coolector.Common.Commands;
using Coolector.Common.Events.Facebook;
using Coolector.Common.Services;
using Coolector.Services.Users.Services;
using Coolector.Services.Users.Shared;
using Coolector.Services.Users.Shared.Commands.Facebook;
using RawRabbit;

namespace Coolector.Services.Users.Handlers
{
    public class PostMessageOnFacebookWallHandler : ICommandHandler<PostMessageOnFacebookWall>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IFacebookService _facebookService;

        public PostMessageOnFacebookWallHandler(IHandler handler,
            IBusClient bus,
            IFacebookService facebookService)
        {
            _handler = handler;
            _bus = bus;
            _facebookService = facebookService;
        }

        public async Task HandleAsync(PostMessageOnFacebookWall command)
        {
            await _handler
                .Run(async () => await _facebookService.PostOnWallAsync(command.AccessToken, command.Message))
                .OnSuccess(async () => await _bus.PublishAsync(new MessageOnFacebookWallPosted(command.Request.Id,
                    command.UserId, command.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error occured while posting message on facebook wall");
                    await _bus.PublishAsync(new PostMessageOnFacebookWallRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message, command.Message));
                })
                .ExecuteAsync();
        }
    }
}