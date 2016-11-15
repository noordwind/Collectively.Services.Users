using System;
using System.Threading.Tasks;
using Coolector.Common.Mongo;
using Coolector.Services.Users.Domain;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Coolector.Services.Users.Repositories.Queries
{
    public static class UserSessionQueries
    {
        public static IMongoCollection<UserSession> UserSessions(this IMongoDatabase database)
            => database.GetCollection<UserSession>();

        public static async Task<UserSession> GetByIdAsync(this IMongoCollection<UserSession> sessions, Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return await sessions.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}