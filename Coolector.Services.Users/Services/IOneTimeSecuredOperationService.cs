using System;
using System.Threading.Tasks;
using Coolector.Common.Types;
using Coolector.Services.Users.Domain;

namespace Coolector.Services.Users.Services
{
    public interface IOneTimeSecuredOperationService
    {
        Task<Maybe<OneTimeSecuredOperation>> GetAsync(Guid id);
        Task CreateAsync(Guid id, string type, string user, DateTime expiry);
        Task<bool> CanBeConsumedAsync(string type, string user, string token);
        Task ConsumeAsync(string type, string user, string token);
    }
}