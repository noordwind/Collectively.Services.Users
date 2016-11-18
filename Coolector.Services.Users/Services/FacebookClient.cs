using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Coolector.Services.Users.Services
{
    public class FacebookClient : IFacebookClient
    {
        private readonly HttpClient _httpClient;

        public FacebookClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/v2.8/")
            };
        }

        public async Task<T> GetAsync<T>(string endpoint, string accessToken)
        {
            var response = await _httpClient.GetAsync($"{endpoint}&access_token={accessToken}");
            if (!response.IsSuccessStatusCode)
                return default(T);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }

        public async Task PostAsync(string endpoint, dynamic data, string accessToken)
        {
            var response = await _httpClient.PostAsync($"{endpoint}&access_token={accessToken}", data);
        }
    }
}