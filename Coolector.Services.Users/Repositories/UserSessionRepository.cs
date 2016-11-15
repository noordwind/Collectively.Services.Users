using System;
using System.Threading.Tasks;
using Coolector.Common.Types;
using Coolector.Services.Users.Domain;
using Coolector.Services.Users.Repositories.Queries;
using MongoDB.Driver;

namespace Coolector.Services.Users.Repositories
{
    public class UserSessionRepository : IUserSessionRepository
    {
        private readonly IMongoDatabase _database;

        public UserSessionRepository(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<Maybe<UserSession>> GetByIdAsync(Guid id)
            => await _database.UserSessions().GetByIdAsync(id);

        public async Task AddAsync(UserSession session)
            => await _database.UserSessions().InsertOneAsync(session);

        public async Task UpdateAsync(UserSession session)
            => await _database.UserSessions().ReplaceOneAsync(x => x.Id == session.Id, session);
    }
}