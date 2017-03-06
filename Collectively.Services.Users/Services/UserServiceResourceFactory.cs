namespace Collectively.Services.Users.Services
{
    public class UserServiceResourceFactory : IResourceFactory
    {
        private static readonly IDictionary<Type, Tuple<string, string>> _resources = 

        public Resource Create<T>(params object[] args) where T : class
        {

        }
    }
}