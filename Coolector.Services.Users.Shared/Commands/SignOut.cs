using System;
using Coolector.Common.Commands;

namespace Coolector.Services.Users.Shared.Commands
{
    public class SignOut : IAuthenticatedCommand
    {
        public Request Request { get; set; }
        public Guid SessionId { get; set; }
        public string UserId { get; set; }
    }
}