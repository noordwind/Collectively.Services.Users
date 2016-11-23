using System.Threading.Tasks;

namespace Coolector.Services.Users.Services
{
    public interface IPasswordService
    {
        Task ChangeAsync(string userId, string currentPassword, string newPassword);
    }
}