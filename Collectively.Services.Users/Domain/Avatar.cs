using System;
using Collectively.Common.Domain;
using Collectively.Common.Extensions;

namespace Collectively.Services.Users.Domain
{
    public class Avatar : ValueObject<Avatar>
    {
        public string Name { get; protected set; }
        public string Size { get; protected set; }
        public string Url { get; protected set; }

        protected Avatar()
        {
        }

        protected Avatar(string name, string size, string url)
        {
            if (name.Empty())
            {
                throw new ArgumentException("Avatar name can not be empty.", nameof(size));
            }
            if (size.Empty())
            {
                throw new ArgumentException("Avatar size can not be empty.", nameof(size));
            }
            if (url.Empty())
            {
                throw new ArgumentException("Avatar Url can not be empty.", nameof(url));
            }

            Name = name;
            Size = size;
            Url = url;
        }

        public static Avatar Empty => new Avatar();

        public static Avatar Create(string name, string size, string url)
            => new Avatar(name, size, url);

        protected override bool EqualsCore(Avatar other) => Name.Equals(other.Name);

        protected override int GetHashCodeCore() => Name.GetHashCode();
    }
}