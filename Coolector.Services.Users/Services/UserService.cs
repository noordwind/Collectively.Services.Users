using System.Threading.Tasks;
using Coolector.Common.Extensions;
using Coolector.Common.Types;
using Coolector.Common.Domain;
using Coolector.Common.Services;
using Coolector.Services.Users.Domain;
using Coolector.Services.Users.Queries;
using Coolector.Services.Users.Repositories;
using Coolector.Services.Users.Shared;

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

        public async Task<bool> IsNameAvailableAsync(string name)
            => await _userRepository.ExistsAsync(name.ToLowerInvariant()) == false;

        public async Task<Maybe<User>> GetAsync(string userId)
            => await _userRepository.GetByUserIdAsync(userId);

        public async Task<Maybe<User>> GetByNameAsync(string name)
            => await _userRepository.GetByNameAsync(name);

        public async Task<Maybe<User>> GetByExternalUserIdAsync(string externalUserId)
            => await _userRepository.GetByExternalUserIdAsync(externalUserId);

        public async Task<Maybe<User>> GetByEmailAsync(string email, string provider)
            => await _userRepository.GetByEmailAsync(email, provider);

        public async Task<Maybe<PagedResult<User>>> BrowseAsync(BrowseUsers query)
            => await _userRepository.BrowseAsync(query);

        public async Task SignUpAsync(string userId, string email, string role,
            string provider, string password = null, string externalUserId = null,
            bool activate = true, string pictureUrl = null, string name = null)
        {
            var user = await _userRepository.GetByUserIdAsync(userId);
            if (user.HasValue)
            {
                throw new ServiceException(OperationCodes.UserIdInUse,
                    $"User with id: '{userId}' already exists.");
            }
            user = await _userRepository.GetByEmailAsync(email, provider);
            if (user.HasValue)
            {
                throw new ServiceException(OperationCodes.EmailInUse,
                    $"User with email: {email} already exists!");
            }

            user = await _userRepository.GetByNameAsync(name);
            if (user.HasValue)
            {
                throw new ServiceException(OperationCodes.NameInUse,
                    $"User with name: {name} already exists!");
            }

            if (provider == Providers.Coolector && password.Empty())
            {
                throw new ServiceException(OperationCodes.InvalidPassword,
                    $"Password can not be empty!");

            }

            user = new User(userId, email, role, provider, pictureUrl);
            if (!password.Empty())
                user.Value.SetPassword(password, _encrypter);
            if (name.NotEmpty())
            {
                user.Value.SetName(name);
                if (activate)
                    user.Value.Activate();
            }
            if (externalUserId.NotEmpty())
                user.Value.SetExternalUserId(externalUserId);

            await _userRepository.AddAsync(user.Value);
        }

        public async Task ChangeNameAsync(string userId, string name)
        {
            var user = await GetAsync(userId);
            if (user.HasNoValue)
            {
                throw new ServiceException(OperationCodes.UserNotFound,
                    $"User with id: '{userId}' has not been found.");
            }
            if (await IsNameAvailableAsync(name) == false)
            {
                throw new ServiceException(OperationCodes.NameInUse,
                    $"User with name: '{name}' already exists.");
            }

            user.Value.SetName(name);
            user.Value.Activate();
            await _userRepository.UpdateAsync(user.Value);
        }

        public async Task ChangeAvatarAsync(string userId, string pictureUrl)
        {
            var user = await GetAsync(userId);
            if (user.HasNoValue)
            {
                throw new ServiceException(OperationCodes.UserNotFound,
                    $"User with id: '{userId}' has not been found.");
            }

            user.Value.SetAvatar(pictureUrl);
            await _userRepository.UpdateAsync(user.Value);
        }
    }
}