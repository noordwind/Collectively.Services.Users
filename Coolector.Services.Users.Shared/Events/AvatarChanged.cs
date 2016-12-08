using System;
using Coolector.Common.Events;

namespace Coolector.Services.Users.Shared.Events
{
    public class AvatarChanged : IAuthenticatedEvent
    {
        public Guid RequestId { get; }
        public string UserId { get;}
        public string PictureUrl { get; }

        protected AvatarChanged()
        {
        }

        public AvatarChanged(Guid requestId, string userId, string pictureUrl)
        {
            RequestId = requestId;
            UserId = userId;
            PictureUrl = pictureUrl;
        }
    }
}