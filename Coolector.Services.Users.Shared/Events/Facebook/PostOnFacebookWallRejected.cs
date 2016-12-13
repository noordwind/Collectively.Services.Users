using System;
using Coolector.Common.Events;

namespace Coolector.Services.Users.Shared.Events.Facebook
{
    public class PostOnFacebookWallRejected : IRejectedEvent
    {
        public Guid RequestId { get; }
        public string UserId { get; }
        public string Code { get; }
        public string Reason { get; }
        public string Message { get; }

        protected PostOnFacebookWallRejected()
        {
        }

        public PostOnFacebookWallRejected(Guid requestId,
            string userId, string code,
            string reason, string message)
        {
            RequestId = requestId;
            UserId = userId;
            Code = code;
            Reason = reason;
            Message = message;
        }
    }
}