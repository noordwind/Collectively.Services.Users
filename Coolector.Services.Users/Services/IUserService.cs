using System.Threading.Tasks;
using Coolector.Common.Types;
using Coolector.Services.Users.Domain;
using Coolector.Services.Users.Queries;

namespace Coolector.Services.Users.Services
{
    public interface IUserService
    {
        Task<bool> IsNameAvailableAsync(string name);
        Task<Maybe<User>> GetAsync(string userId);
        Task<Maybe<User>> GetByNameAsync(string name);
        Task<Maybe<User>> GetByExternalUserIdAsync(string externalUserId);
        Task<Maybe<User>> GetByEmailAsync(string email, string provider);
        Task<Maybe<PagedResult<User>>> BrowseAsync(BrowseUsers query);

        Task SignUpAsync(string userId, string email, string role,
            string provider, string password = null,
            string externalUserId = null,
            bool activate = true, string pictureUrl = null,
            string name = null);

        Task ChangeNameAsync(string userId, string name);
        Task ChangeAvatarAsync(string userId, string name);
    }
}