using Collectively.Common.Queries;

namespace Collectively.Services.Users.Queries
{
    public class GetUserByName : IQuery
    {
        public string Name { get; set; }
    }
}