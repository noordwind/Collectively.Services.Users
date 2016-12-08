using Coolector.Common.Commands;

namespace Coolector.Services.Users.Shared.Commands
{
    public class ChangeAvatar : IAuthenticatedCommand
    {
        public Request Request { get; set; }
        public string UserId { get; set; }
        public string PictureUrl { get; set; }
    }
}