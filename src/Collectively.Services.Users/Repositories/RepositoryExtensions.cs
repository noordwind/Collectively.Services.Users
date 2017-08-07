using System.Threading.Tasks;
using Collectively.Common.Domain;
using Collectively.Common.Extensions;
using Collectively.Services.Users.Domain;

namespace Collectively.Services.Users.Repositories
{
    public static class RepositoryExtensions
    {
        public static async Task<User> GetOrFailAsync(this IUserRepository repository,string userId)
            => await repository
                .GetByUserIdAsync(userId)
                .UnwrapAsync(noValueException: new ServiceException(OperationCodes.UserNotFound,
                    $"User with id: '{userId}' does not exist!"));
    }
}