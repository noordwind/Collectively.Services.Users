using AutoMapper;
using Coolector.Common.Dto.General;
using Coolector.Common.Dto.Users;
using Coolector.Services.Users.Domain;

namespace Coolector.Services.Users.Framework
{
    public class AutoMapperConfig
    {
        public static IMapper InitializeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDto>();
                cfg.CreateMap<dynamic, AvailableResourceDto>()
                    .ForMember(x => x.IsAvailable, o => o.MapFrom(s => s));
                cfg.CreateMap<UserSession, UserSessionDto>();
            });

            return config.CreateMapper();
        }
    }
}