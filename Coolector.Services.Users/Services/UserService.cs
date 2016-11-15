using System.Threading.Tasks;
using Coolector.Common.Extensions;
using Coolector.Common.Types;
using Coolector.Common.Domain;
using Coolector.Common.Encryption;
using Coolector.Services.Users.Domain;
using Coolector.Services.Users.Queries;
using Coolector.Services.Users.Repositories;

namespace Coolector.Services.Users.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEncrypter _encrypter;

        public UserService(IUserRepository userRepository, IEncrypter encrypter)
        {
            _userRepository = userRepository;
            _encrypter = encrypter;
        }

        public async Task<Maybe<User>> GetAsync(string userId)
            => await _userRepository.GetByUserIdAsync(userId);

        public async Task<Maybe<User>> GetByNameAsync(string name)
            => await _userRepository.GetByNameAsync(name);

        public async Task<Maybe<User>> GetByEmailAsync(string email, string provider)
            => await _userRepository.GetByEmailAsync(email, provider);

        public async Task<Maybe<PagedResult<User>>> BrowseAsync(BrowseUsers query)
            => await _userRepository.BrowseAsync(query);

        public async Task SignUpAsync(string userId, string email, string role,
            string provider, string password = null,
            bool activate = true, string pictureUrl = null, string name = null)
        {
            var user = await _userRepository.GetByUserIdAsync(userId);
            if (user.HasValue)
                throw new ServiceException($"User with id: {userId} already exists!");

            user = await _userRepository.GetByEmailAsync(email,provider);
            if (user.HasValue)
                throw new ServiceException($"User with email: {email} already exists!");

            user = new User(userId, email, role, provider, pictureUrl);
            if(provider == Providers.Coolector && password.Empty())
                throw new ServiceException($"Password can not be empty!");

            if(!password.Empty())
                user.Value.SetPassword(password, _encrypter);
            if (activate)
                user.Value.Activate();

            user.Value.SetName(name.Empty() ? $"user-{user.Value.Id:N}" : name);

            await _userRepository.AddAsync(user.Value);
        }

        public async Task ChangeNameAsync(string userId, string name)
        {
            var user = await GetAsync(userId);
            if (user.HasNoValue)
                throw new ServiceException($"User with id {userId} has not been found.");

            user.Value.SetName(name);
            await _userRepository.UpdateAsync(user.Value);
        }

        public async Task ChangeAvatarAsync(string userId, string pictureUrl)
        {
            var user = await GetAsync(userId);
            if (user.HasNoValue)
                throw new ServiceException($"User with id {userId} has not been found.");

            user.Value.SetAvatar(pictureUrl);
            await _userRepository.UpdateAsync(user.Value);
        }
    }
}