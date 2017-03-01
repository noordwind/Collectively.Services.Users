using System.Threading.Tasks;
using Collectively.Common.Extensions;
using Collectively.Common.Mongo;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Queries;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Collectively.Services.Users.Repositories.Queries
{
    public static class UserQueries
    {
        public static IMongoCollection<User> Users(this IMongoDatabase database)
            => database.GetCollection<User>();

        public static async Task<bool> ExistsAsync(this IMongoCollection<User> users, string name)
            => await users.AsQueryable().AnyAsync(x => x.Name == name);

        public static async Task<User> GetByUserIdAsync(this IMongoCollection<User> users, string userId)
        {
            if (userId.Empty())
                return null;

            return await users.AsQueryable().FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public static async Task<User> GetByExternalUserIdAsync(this IMongoCollection<User> users, string externalUserId)
        {
            if (externalUserId.Empty())
                return null;

            return await users.AsQueryable().FirstOrDefaultAsync(x => x.ExternalUserId == externalUserId);
        }

        public static async Task<User> GetByEmailAsync(this IMongoCollection<User> users, string email, string provider)
        {
            if (email.Empty())
                return null;
            if (provider.Empty())
                return null;

            return await users.AsQueryable().FirstOrDefaultAsync(x => x.Email == email && x.Provider == provider);
        }

        public static async Task<User> GetByNameAsync(this IMongoCollection<User> users, string name)
        {
            if (name.Empty())
                return null;

            return await users.AsQueryable().FirstOrDefaultAsync(x => x.Name == name);
        }

        public static IMongoQueryable<User> Query(this IMongoCollection<User> users,
            BrowseUsers query)
        {
            var values = users.AsQueryable();

            return values.OrderBy(x => x.Name);
        }
    }
}