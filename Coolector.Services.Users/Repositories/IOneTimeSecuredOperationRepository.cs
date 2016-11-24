using System.Threading.Tasks;
using Coolector.Common.Types;
using Coolector.Services.Users.Domain;

namespace Coolector.Services.Users.Repositories
{
    public interface IOneTimeSecuredOperationRepository
    {
        Task<Maybe<OneTimeSecuredOperation>> GetAsync(string type, string user, string token);
        Task AddAsync(OneTimeSecuredOperation operation);
        Task UpdateAsync(OneTimeSecuredOperation operation);
    }
}