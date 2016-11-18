using System;
using System.Threading.Tasks;
using Coolector.Common.Commands;
using Coolector.Common.Commands.Facebook;
using Coolector.Common.Events.Facebook;
using Coolector.Services.Users.Services;
using RawRabbit;

namespace Coolector.Services.Users.Handlers
{
    public class PostMessageOnFacebookWallHandler : ICommandHandler<PostMessageOnFacebookWall>
    {
        private readonly IBusClient _bus;
        private readonly IUserService _userService;
        private readonly IFacebookService _facebookService;

        public PostMessageOnFacebookWallHandler(IBusClient bus, IUserService userService,
            IFacebookService facebookService)
        {
            _bus = bus;
            _userService = userService;
            _facebookService = facebookService;
        }

        public async Task HandleAsync(PostMessageOnFacebookWall command)
        {
            try
            {
                await _facebookService.PostOnWallAsync(command.AccessToken, command.Message);
                await _bus.PublishAsync(new MessageOnFacebookWallPosted(command.Request.Id,
                    command.UserId, command.Message));
            }
            catch (Exception exception)
            {
                await _bus.PublishAsync(new PostMessageOnFacebookWallRejected(command.Request.Id,
                    command.UserId, exception.Message, command.Message));
            }
        }
    }
}