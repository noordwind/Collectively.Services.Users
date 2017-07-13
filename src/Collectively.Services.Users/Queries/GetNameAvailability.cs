using Collectively.Common.Queries;

namespace Collectively.Services.Users.Queries
{
    public class GetNameAvailability : IQuery
    {
        public string Name { get; set; }
    }
}