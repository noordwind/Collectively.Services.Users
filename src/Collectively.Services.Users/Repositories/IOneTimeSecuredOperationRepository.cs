using System;
using System.Threading.Tasks;
using Collectively.Common.Types;
using Collectively.Services.Users.Domain;

namespace Collectively.Services.Users.Repositories
{
    public interface IOneTimeSecuredOperationRepository
    {
        Task<Maybe<OneTimeSecuredOperation>> GetAsync(Guid id);
        Task<Maybe<OneTimeSecuredOperation>> GetAsync(string type, string user, string token);
        Task AddAsync(OneTimeSecuredOperation operation);
        Task UpdateAsync(OneTimeSecuredOperation operation);
    }
}