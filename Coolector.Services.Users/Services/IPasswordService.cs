using System;
using System.Threading.Tasks;

namespace Coolector.Services.Users.Services
{
    public interface IPasswordService
    {
        Task ChangeAsync(string userId, string currentPassword, string newPassword);
        Task ResetAsync(Guid operationId, string email);
        Task SetNewAsync(string email, string token, string password);
    }
}