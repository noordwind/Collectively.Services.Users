using System;
using System.Threading.Tasks;

namespace Coolector.Services.Users.Services
{
    public interface IOneTimeSecuredOperationService
    {
        Task CreateAsync(Guid id, string type, string user, DateTime expiry);
        Task<bool> CanBeConsumedAsync(string type, string user, string token);
        Task ConsumeAsync(string type, string user, string token);
    }
}