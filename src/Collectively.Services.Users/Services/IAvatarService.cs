using System.Threading.Tasks;
using Collectively.Common.Files;

namespace Collectively.Services.Users.Services
{
    public interface IAvatarService
    {
         Task<string> GetUrlAsync(string userId);
         Task AddOrUpdateAsync(string userId, File avatar);
         Task RemoveAsync(string userId);
    }
}