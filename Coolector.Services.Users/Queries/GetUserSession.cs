using System;
using Coolector.Common.Queries;

namespace Coolector.Services.Users.Queries
{
    public class GetUserSession : IQuery
    {
        public Guid Id { get; set; }
    }
}