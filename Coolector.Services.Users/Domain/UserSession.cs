using System;
using Coolector.Common.Domain;

namespace Coolector.Services.Users.Domain
{
    public class UserSession
    {
        public Guid Id { get; protected set; }
        public string UserId { get; protected set; }
        public string Key { get; protected set; }
        public string UserAgent { get; protected set; }
        public string IpAddress { get; protected set; }
        public Guid? ParentId { get; protected set; }
        public bool Refreshed { get; protected set; }
        public bool Destroyed { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }

        protected UserSession()
        {
        }

        public UserSession(Guid id, string userId) : this(id, userId, null)
        {
        }

        public UserSession(Guid id, string userId, string key,
            string ipAddress = null, string userAgent = null,
            Guid? parentId = null)
        {
            Id = id;
            UserId = userId;
            Key = key;
            UserAgent = userAgent;
            IpAddress = ipAddress;
            ParentId = parentId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Destroy()
        {
            CheckIfAlreadyRefreshedOrDestroyed();
            Destroyed = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public UserSession Refresh(Guid id, string key, Guid parentId,
            string ipAddress = null, string userAgent = null)
        {
            CheckIfAlreadyRefreshedOrDestroyed();
            ParentId = parentId;
            Refreshed = true;
            UpdatedAt = DateTime.UtcNow;

            return new UserSession(id, UserId, key, ipAddress, userAgent, parentId);
        }

        private void CheckIfAlreadyRefreshedOrDestroyed()
        {
            if (Refreshed)
            {
                throw new DomainException(OperationCodes.SessionExpired,
                    $"Session for user id: '{UserId}' " +
                    $"with key: '{Key}' has been already refreshed.");
            }
            if (Destroyed)
            {
                throw new DomainException(OperationCodes.SessionExpired,
                    $"Session for user id: '{UserId}' " +
                    $"with key: '{Key}' has been already destroyed.");
            }
        }
    }
}