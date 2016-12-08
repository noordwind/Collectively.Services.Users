using System;
using Coolector.Common.Events;

namespace Coolector.Services.Users.Shared.Events
{
    public class SignOutRejected : IRejectedEvent
    {
        public Guid RequestId { get; }
        public string UserId { get; }
        public string Code { get; }
        public string Reason { get; }

        protected SignOutRejected() { }

        public SignOutRejected(Guid requestId, string userId, string code, string reason)
        {
            RequestId = requestId;
            UserId = userId;
            Code = code;
            Reason = reason;
        }
    }
}