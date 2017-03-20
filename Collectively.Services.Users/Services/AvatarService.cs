using System.Threading.Tasks;
using Collectively.Common.Domain;
using Collectively.Common.Files;
using Collectively.Services.Users.Repositories;
using NLog;

namespace Collectively.Services.Users.Services
{
    public class AvatarService : IAvatarService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IUserRepository _userRepository;
        private readonly IFileHandler _fileHandler;
        private readonly IImageService _imageService;

        public AvatarService(IUserRepository userRepository, 
            IFileHandler fileHandler, IImageService imageService)
        {
            _userRepository = userRepository;
            _fileHandler = fileHandler;
            _imageService = imageService;
        }

        public async Task UploadAvatarAsync(string userId, File avatar)
        {
            if (avatar == null)
            {
                throw new ServiceException(OperationCodes.InvalidAvatar, 
                    $"There is no avatar file to be uploaded.");
            }
            var resizedAvatar = _imageService.ProcessImage(avatar, 100);
        }

        public async Task RemoveAvatarAsync(string userId)
        {
        }
    }
}