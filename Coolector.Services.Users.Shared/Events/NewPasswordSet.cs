using System;
using Coolector.Common.Events;

namespace Coolector.Services.Users.Shared.Events
{
    public class NewPasswordSet : IEvent
    {
        public Guid RequestId { get; }
        public string Email { get; }

        protected NewPasswordSet()
        {
        }

        public NewPasswordSet(Guid requestId, string email)
        {
            RequestId = requestId;
            Email = email;
        }
    }
}