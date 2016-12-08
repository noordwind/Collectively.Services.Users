using System;
using Coolector.Common.Events;

namespace Coolector.Services.Users.Shared.Events
{
    public class UserSignedOut : IEvent
    {
        public Guid RequestId { get; }
        public string UserId { get; }
        public Guid SessionId { get; }

        protected UserSignedOut()
        {
        }

        public UserSignedOut(Guid requestId, string userId, Guid sessionId)
        {
            RequestId = requestId;
            UserId = userId;
            SessionId = sessionId;
        }
    }
}