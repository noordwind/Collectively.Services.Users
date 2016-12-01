using System;
using System.Text.RegularExpressions;
using Coolector.Common.Extensions;
using Coolector.Common.Domain;
using Coolector.Common.Services;

namespace Coolector.Services.Users.Domain
{
    public class User : IdentifiableEntity, ITimestampable
    {
        private static readonly Regex NameRegex = new Regex("^(?![_.-])(?!.*[_.-]{2})[a-zA-Z0-9._.-]+(?<![_.-])$",
            RegexOptions.Compiled);

        public string UserId { get; protected set; }
        public string Email { get; protected set; }
        public string Name { get; protected set; }
        public string Password { get; protected set; }
        public string Salt { get; protected set; }
        public string Provider { get; protected set; }
        public string PictureUrl { get; protected set; }
        public string Role { get; protected set; }
        public string State { get; protected set; }
        public string ExternalUserId { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }

        protected User()
        {
        }

        public User(string userId, string email, string role, string provider, string pictureUrl = null)
        {
            SetUserId(userId);
            SetEmail(email);
            Provider = provider;
            Role = role;
            PictureUrl = pictureUrl;
            State = States.Incomplete;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            UserId = userId;
            Name = $"user-{Id:N}";
        }

        public void SetUserId(string userId)
        {
            if (userId.Empty())
                throw new ArgumentException("User id can not be empty.", nameof(userId));

            UserId = userId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetExternalUserId(string externalUserId)
        {
            ExternalUserId = externalUserId;
            UpdatedAt = DateTime.UtcNow;
        }

        //TODO: Might not work with FB etc.
        public void SetEmail(string email)
        {
            if (email.Empty())
            {
                Email = string.Empty;
                UpdatedAt = DateTime.UtcNow;

                return;
            }
            if (!email.IsEmail())
                throw new ArgumentException($"Invalid email {email}.", nameof(email));
            if (Email.EqualsCaseInvariant(email))
                return;

            Email = email.ToLowerInvariant();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetName(string name)
        {
            if (State != States.Incomplete)
                throw new DomainException($"User name has been already set: {Name}");
            if (name.Empty())
                throw new ArgumentException("User name can not be empty.", nameof(name));
            if (Name.EqualsCaseInvariant(name))
                return;
            if (name.Length < 2)
                throw new ArgumentException("User name is too short.", nameof(name));
            if (name.Length > 50)
                throw new ArgumentException("User name is too long.", nameof(name));
            if (NameRegex.IsMatch(name) == false)
                throw new ArgumentException("User name doesn't meet the required criteria.", nameof(name));

            Name = name.ToLowerInvariant();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetRole(string role)
        {
            if (Role == role)
                return;

            Role = role;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Lock()
        {
            if (State == States.Locked)
                return;

            State = States.Locked;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (State == States.Active)
                return;

            State = States.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAvatar(string pictureUrl)
        {
            PictureUrl = pictureUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPassword(string password, IEncrypter encrypter)
        {
            if (password.Empty())
                throw new DomainException("Password can not be empty.");
            if (password.Length < 4)
                throw new DomainException("Password must contain at least 4 characters.");
            if (password.Length > 100)
                throw new DomainException("Password can not contain more than 100 characters.");

            var salt = encrypter.GetSalt(password);
            var hash = encrypter.GetHash(password, salt);

            Password = hash;
            Salt = salt;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool ValidatePassword(string password, IEncrypter encrypter)
        {
            var hashedPassword = encrypter.GetHash(password, Salt);
            var areEqual = Password.Equals(hashedPassword);

            return areEqual;
        }
    }
}