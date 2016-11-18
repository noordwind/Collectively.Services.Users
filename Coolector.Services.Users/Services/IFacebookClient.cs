using System.Threading.Tasks;

namespace Coolector.Services.Users.Services
{
    public interface IFacebookClient
    {
        Task<T> GetAsync<T>(string endpoint, string accessToken);
        Task PostAsync(string endpoint, dynamic data, string accessToken);
    }
}