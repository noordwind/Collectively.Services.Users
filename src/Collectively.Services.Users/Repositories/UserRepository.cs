using System.Threading.Tasks;
using Collectively.Common.Types;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Queries;
using MongoDB.Driver;
using Collectively.Common.Mongo;
using Collectively.Services.Users.Repositories.Queries;
using System;

namespace Collectively.Services.Users.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoDatabase _database;

        public UserRepository(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<bool> ExistsAsync(string name)
            => await _database.Users().ExistsAsync(name);

        public async Task<Maybe<User>> GetByUserIdAsync(string userId)
            => await _database.Users().GetByUserIdAsync(userId);

        public async Task<Maybe<User>> GetByExternalUserIdAsync(string externalUserId)
            => await _database.Users().GetByExternalUserIdAsync(externalUserId);

        public async Task<Maybe<User>> GetByEmailAsync(string email, string provider)
            => await _database.Users().GetByEmailAsync(email, provider);

        public async Task<Maybe<User>> GetByNameAsync(string name)
            => await _database.Users().GetByNameAsync(name);

        public async Task<Maybe<PagedResult<User>>> BrowseAsync(BrowseUsers query)
        {
            return await _database.Users()
                .Query(query)
                .PaginateAsync(query);
        }

        public async Task AddAsync(User user)
            => await _database.Users().InsertOneAsync(user);

        public async Task UpdateAsync(User user)
            => await _database.Users().ReplaceOneAsync(x => x.Id == user.Id, user);

        public async Task DeleteAsync(string userId)
            => await _database.Users().DeleteOneAsync(x => x.UserId == userId);
    }
}