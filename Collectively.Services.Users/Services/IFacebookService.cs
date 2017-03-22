using System.Threading.Tasks;
using Collectively.Common.Types;
using Collectively.Services.Users.Domain;

namespace Collectively.Services.Users.Services
{
    public interface IFacebookService
    {
        Task<Maybe<FacebookUser>> GetUserAsync(string accessToken);
        Task<bool> ValidateTokenAsync(string accessToken);
        Task PostOnWallAsync(string accessToken, string message);
        string GetAvatarUrl(string facebookId);
    }
}