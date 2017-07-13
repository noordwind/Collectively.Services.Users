using System.Threading.Tasks;
using Collectively.Common.Types;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Queries;

namespace Collectively.Services.Users.Services
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
            bool activate = true, string name = null,
            string culture = "en-gb");

        Task ChangeNameAsync(string userId, string name);
    }
}