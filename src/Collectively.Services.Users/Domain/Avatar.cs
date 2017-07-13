using System;
using Collectively.Common.Domain;
using Collectively.Common.Extensions;

namespace Collectively.Services.Users.Domain
{
    public class Avatar : ValueObject<Avatar>
    {
        public string Name { get; protected set; }
        public string Url { get; protected set; }
        public bool IsEmpty => Name.Empty();

        protected Avatar()
        {
        }

        protected Avatar(string name, string url)
        {
            if (name.Empty())
            {
                throw new ArgumentException("Avatar name can not be empty.", nameof(name));
            }
            if (url.Empty())
            {
                throw new ArgumentException("Avatar Url can not be empty.", nameof(url));
            }
            Name = name;
            Url = url;
        }

        public static Avatar Empty => new Avatar();

        public static Avatar Create(string name, string url)
            => new Avatar(name, url);

        protected override bool EqualsCore(Avatar other) => Name.Equals(other.Name);

        protected override int GetHashCodeCore() => Name.GetHashCode();
    }
}