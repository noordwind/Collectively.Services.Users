using System.Threading.Tasks;

namespace Coolector.Services.Users.Services
{
    public interface IEmailMessenger
    {
        Task SendPasswordResetAsync(string email, string token);
    }
}