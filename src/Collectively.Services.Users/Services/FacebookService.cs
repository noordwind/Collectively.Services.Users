using System;
using System.Threading.Tasks;
using Collectively.Common.Types;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Settings;

namespace Collectively.Services.Users.Services
{
    public class FacebookService : IFacebookService
    {
        private readonly IFacebookClient _facebookClient;
        private readonly FacebookSettings _facebookSettings;

        public FacebookService(IFacebookClient facebookClient, FacebookSettings facebookSettings)
        {
            _facebookClient = facebookClient;
            _facebookSettings = facebookSettings;
        }

        public async Task<Maybe<FacebookUser>> GetUserAsync(string accessToken)
        {
            var user = await _facebookClient.GetAsync<dynamic>(
                "me", accessToken, "fields=id,name,email,first_name,last_name,age_range,birthday,gender,locale");
            if (user == null)
                return new Maybe<FacebookUser>();

            var facebookUser = new FacebookUser
            {
                Id = user.id,
                Email = user.email,
                Name = user.name,
                UserName = user.username,
                FirstName = user.first_name,
                LastName = user.last_name,
                PublicToken = accessToken,
                Locale = user.locale
            };

            return facebookUser;
        }

        public async Task<bool> ValidateTokenAsync(string accessToken)
        {
            try
            {
                var user = await GetUserAsync(accessToken);

                return user.HasValue;
            }
            catch
            {
                return false;
            }
        }

        public async Task PostOnWallAsync(string accessToken, string message)
            => await _facebookClient.PostAsync("me/feed", accessToken, new {message});

        public string GetAvatarUrl(string facebookId)
            => $"https://graph.facebook.com/{facebookId}/picture?type=large";
  }
}