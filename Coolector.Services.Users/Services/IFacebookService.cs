using System.Threading.Tasks;
using Coolector.Common.Types;
using Coolector.Services.Users.Domain;

namespace Coolector.Services.Users.Services
{
    public interface IFacebookService
    {
        Task<Maybe<FacebookUser>> GetUserAsync(string accessToken);
        Task<bool> ValidateTokenAsync(string accessToken);
        Task PostOnWallAsync(string accessToken, string message);
    }
}