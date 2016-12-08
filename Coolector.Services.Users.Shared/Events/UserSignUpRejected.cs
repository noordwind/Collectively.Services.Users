using System;
using Coolector.Common.Events;

namespace Coolector.Services.Users.Shared.Events
{
    public class UserSignUpRejected : IRejectedEvent
    {
        public Guid RequestId { get; }
        public string UserId { get; }
        public string Code { get; }
        public string Reason { get; }
        public string Provider { get; }

        protected UserSignUpRejected()
        {
        }

        public UserSignUpRejected(Guid requestId,
            string userId, string code,
            string reason, string provider)
        {
            RequestId = requestId;
            UserId = userId;
            Code = code;
            Reason = reason;
            Provider = provider;
        }
    }
}