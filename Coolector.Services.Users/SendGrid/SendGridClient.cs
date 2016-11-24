using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Coolector.Common.Domain;
using Newtonsoft.Json;
using NLog;

namespace Coolector.Services.Users.SendGrid
{
    public class SendGridClient : ISendGridClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly SendGridSettings _sendGridSettings;
        private readonly HttpClient _httpClient;

        public SendGridClient(SendGridSettings sendGridSettings)
        {
            _sendGridSettings = sendGridSettings;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(sendGridSettings.ApiUrl)
            };
        }

        public async Task SendMessageAsync(SendGridEmailMessage message)
        {
            Logger.Info("Sending an email message via SendGrid.");
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_sendGridSettings.ApiKey}");
            var payload = JsonConvert.SerializeObject(message);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("mail/send", content);
            if (response.IsSuccessStatusCode)
            {
                Logger.Info("An email message has been sent successfully via SendGrid.");

                return;
            }
            throw new ServiceException("There was an error while sending an email message via SendGrid." +
                                       $"Status code: {response.StatusCode}, reason: {response.ReasonPhrase}.");
        }
    }
}