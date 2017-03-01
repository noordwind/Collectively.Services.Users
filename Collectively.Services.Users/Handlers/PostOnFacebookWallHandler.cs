using System.Threading.Tasks;
using Collectively.Common.Commands;
using Collectively.Common.Services;
using Collectively.Services.Users.Services;

using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using RawRabbit;

namespace Collectively.Services.Users.Handlers
{
    public class PostOnFacebookWallHandler : ICommandHandler<PostOnFacebookWall>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IFacebookService _facebookService;

        public PostOnFacebookWallHandler(IHandler handler,
            IBusClient bus,
            IFacebookService facebookService)
        {
            _handler = handler;
            _bus = bus;
            _facebookService = facebookService;
        }

        public async Task HandleAsync(PostOnFacebookWall command)
        {
            await _handler
                .Run(async () => await _facebookService.PostOnWallAsync(command.AccessToken, command.Message))
                .OnSuccess(async () => await _bus.PublishAsync(new MessageOnFacebookWallPosted(command.Request.Id,
                    command.UserId, command.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error occured while posting message on facebook wall");
                    await _bus.PublishAsync(new PostOnFacebookWallRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message, command.Message));
                })
                .ExecuteAsync();
        }
    }
}