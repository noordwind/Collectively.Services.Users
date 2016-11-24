using System.Threading.Tasks;
using Coolector.Common.Extensions;
using Coolector.Common.Mongo;
using Coolector.Services.Users.Domain;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Coolector.Services.Users.Repositories.Queries
{
    public static class OneTimeSecuredOperationQueries
    {
        public static IMongoCollection<OneTimeSecuredOperation> OneTimeSecuredOperations(this IMongoDatabase database)
            => database.GetCollection<OneTimeSecuredOperation>();

        public static async Task<OneTimeSecuredOperation> GetAsync(this IMongoCollection<OneTimeSecuredOperation> operations,
            string type, string user, string token)
        {
            if (type.Empty())
                return null;
            if (user.Empty())
                return null;
            if (token.Empty())
                return null;

            return await operations.AsQueryable().FirstOrDefaultAsync(x => x.Type == type &&
                                                                           x.User == user && x.Token == token);
        }
    }
}