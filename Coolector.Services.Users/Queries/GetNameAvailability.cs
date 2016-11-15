using Coolector.Common.Queries;

namespace Coolector.Services.Users.Queries
{
    public class GetNameAvailability : IQuery
    {
        public string Name { get; set; }
    }
}