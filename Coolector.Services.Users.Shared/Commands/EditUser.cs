using Coolector.Common.Commands;

namespace Coolector.Services.Users.Shared.Commands
{
    public class EditUser : IAuthenticatedCommand
    {
        public Request Request { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}