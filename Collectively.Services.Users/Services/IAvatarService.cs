using System.Threading.Tasks;
using Collectively.Common.Files;

namespace Collectively.Services.Users.Services
{
    public interface IAvatarService
    {
         Task UploadAvatarAsync(string userId, File avatar);
         Task RemoveAvatarAsync(string userId);
    }
}