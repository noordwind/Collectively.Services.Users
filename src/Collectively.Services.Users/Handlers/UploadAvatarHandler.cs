using System.Threading.Tasks;
using Collectively.Messages.Commands;
using Collectively.Common.Services;
using Collectively.Services.Users.Services;
using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using RawRabbit;
using Collectively.Common.Files;
using Collectively.Common.Domain;

namespace Collectively.Services.Users.Handlers
{
    public class UploadAvatarHandler : ICommandHandler<UploadAvatar>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IAvatarService _avatarService;
        private readonly IFileResolver _fileResolver;

        public UploadAvatarHandler(IHandler handler, IBusClient bus, 
            IAvatarService avatarService, IFileResolver fileResolver)
        {
            _handler = handler;
            _bus = bus;
            _avatarService = avatarService;
            _fileResolver = fileResolver;
        }

        public async Task HandleAsync(UploadAvatar command)
        {
            var avatarUrl = string.Empty;
            await _handler
                .Run(async () => 
                {
                    var avatar = _fileResolver.FromBase64(command.Avatar.Base64, command.Avatar.Name, command.Avatar.ContentType);
                    if (avatar.HasNoValue)
                    {
                        throw new ServiceException(OperationCodes.InvalidAvatar);
                    }
                    await _avatarService.AddOrUpdateAsync(command.UserId, avatar.Value);
                    avatarUrl = await _avatarService.GetUrlAsync(command.UserId);
                })
                .OnSuccess(async () => await _bus.PublishAsync(new AvatarUploaded(command.Request.Id, 
                    command.UserId, avatarUrl)))
                .OnCustomError(async ex => await _bus.PublishAsync(new UploadAvatarRejected(command.Request.Id,
                        command.UserId, ex.Code, ex.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error occured while uploading avatar.");
                    await _bus.PublishAsync(new UploadAvatarRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message));
                })
                .ExecuteAsync();
        }
    }
}