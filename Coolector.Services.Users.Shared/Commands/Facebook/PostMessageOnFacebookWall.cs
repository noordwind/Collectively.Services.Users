using Coolector.Common.Commands;

namespace Coolector.Services.Users.Shared.Commands.Facebook
{
    public class PostMessageOnFacebookWall : IAuthenticatedCommand
    {
        public Request Request { get; set; }
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public string Message { get; set; }
    }
}