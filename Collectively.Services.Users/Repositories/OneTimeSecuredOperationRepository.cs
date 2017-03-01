using System;
using System.Threading.Tasks;
using Collectively.Common.Types;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Repositories.Queries;
using MongoDB.Driver;

namespace Collectively.Services.Users.Repositories
{
    public class OneTimeSecuredOperationRepository : IOneTimeSecuredOperationRepository
    {
        private readonly IMongoDatabase _database;

        public OneTimeSecuredOperationRepository(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<Maybe<OneTimeSecuredOperation>> GetAsync(Guid id)
            => await _database.OneTimeSecuredOperations().GetAsync(id);

        public async Task<Maybe<OneTimeSecuredOperation>> GetAsync(string type, string user, string token)
            => await _database.OneTimeSecuredOperations().GetAsync(type, user, token);

        public async Task AddAsync(OneTimeSecuredOperation operation)
            => await _database.OneTimeSecuredOperations().InsertOneAsync(operation);

        public async Task UpdateAsync(OneTimeSecuredOperation operation)
            => await _database.OneTimeSecuredOperations().ReplaceOneAsync(x => x.Id == operation.Id, operation);
    }
}