using System;
using Collectively.Common.Queries;

namespace Collectively.Services.Users.Queries
{
    public class GetUserSession : IQuery
    {
        public Guid Id { get; set; }
    }
}