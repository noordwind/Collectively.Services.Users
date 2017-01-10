using System;
using Coolector.Common.Events;

namespace Coolector.Services.Users.Shared.Events
{
    public class SignedOut : IEvent
    {
        public Guid RequestId { get; }
        public string UserId { get; }
        public Guid SessionId { get; }

        protected SignedOut()
        {
        }

        public SignedOut(Guid requestId, string userId, Guid sessionId)
        {
            RequestId = requestId;
            UserId = userId;
            SessionId = sessionId;
        }
    }
}