using System;
using System.Threading.Tasks;
using Coolector.Common.Types;
using Coolector.Services.Users.Domain;

namespace Coolector.Services.Users.Repositories
{
    public interface IUserSessionRepository
    {
        Task<Maybe<UserSession>> GetByIdAsync(Guid id);
        Task AddAsync(UserSession session);
        Task UpdateAsync(UserSession session);
    }
}