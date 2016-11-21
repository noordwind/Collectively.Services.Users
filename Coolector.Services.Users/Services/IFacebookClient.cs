using System.Threading.Tasks;

namespace Coolector.Services.Users.Services
{
    public interface IFacebookClient
    {
        Task<T> GetAsync<T>(string endpoint, string accessToken, string args = null);
        Task PostAsync(string endpoint, string accessToken, dynamic data, string args = null);
    }
}