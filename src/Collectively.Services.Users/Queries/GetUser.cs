using Collectively.Common.Queries;

namespace Collectively.Services.Users.Queries
{
    public class GetUser : IQuery
    {
        public string Id { get; set; }
    }
}