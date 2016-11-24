using System.Threading.Tasks;

namespace Coolector.Services.Users.SendGrid
{
    public interface ISendGridClient
    {
        Task SendMessageAsync(SendGridEmailMessage message);
    }
}