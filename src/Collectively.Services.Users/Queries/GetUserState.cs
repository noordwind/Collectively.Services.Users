using Collectively.Common.Queries;

namespace Collectively.Services.Users.Queries
{
    public class GetUserState : IQuery
    {
        public string Id { get; set; }
    }
}