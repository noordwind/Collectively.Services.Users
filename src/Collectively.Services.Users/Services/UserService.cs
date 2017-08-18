using System.Threading.Tasks;
using Collectively.Common.Extensions;
using Collectively.Common.Types;
using Collectively.Common.Domain;
using Collectively.Common.Services;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Queries;
using Collectively.Services.Users.Repositories;
using System;

namespace Collectively.Services.Users.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEncrypter _encrypter;
        private readonly IOneTimeSecuredOperationService _seruredOperationService;

        public UserService(IUserRepository userRepository,
            IEncrypter encrypter,
            IOneTimeSecuredOperationService seruredOperationService)
        {
            _userRepository = userRepository;
            _encrypter = encrypter;
            _seruredOperationService = seruredOperationService;
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
            bool activate = true, string name = null, string culture = "en-gb")
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
            if (provider == Providers.Collectively && password.Empty())
            {
                throw new ServiceException(OperationCodes.InvalidPassword,
                    $"Password can not be empty!");

            }
            if (!Roles.IsValid(role))
            {
                throw new ServiceException(OperationCodes.InvalidRole, 
                    $"Can not create a new account for user id: '{userId}', invalid role: '{role}'.");
            }
            if (role == Roles.Owner)
            {
                var owner = await _userRepository.GetOwnerAsync();
                if (owner.HasValue)
                {
                    throw new ServiceException(OperationCodes.OwnerAlreadyExists, 
                        $"Can not create a new owner account for user id: '{userId}'.");                    
                }
            }
            user = new User(userId, email, role, provider);
            if (!password.Empty())
                user.Value.SetPassword(password, _encrypter);
            if (name.NotEmpty())
            {
                user.Value.SetName(name);
                if (activate)
                    user.Value.Activate();
                else
                    user.Value.SetUnconfirmed();
            }
            if (externalUserId.NotEmpty())
            {
                user.Value.SetExternalUserId(externalUserId);
            }
            user.Value.SetCulture(culture);
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

        public async Task ActivateAsync(string email, string token)
        {
            await _seruredOperationService.ConsumeAsync(OneTimeSecuredOperations.ActivateAccount,
                email, token);
            var user = await _userRepository.GetByEmailAsync(email, Providers.Collectively);
            if (user.HasNoValue)
            {
                throw new ServiceException(OperationCodes.UserNotFound,
                    $"User with email: '{email}' has not been found.");
            }
            user.Value.Activate();
            await _userRepository.UpdateAsync(user.Value);
        }

        public async Task LockAsync(string userId)
        {
            var user = await _userRepository.GetOrFailAsync(userId);
            if (user.Role == Roles.Owner)
            {
                throw new ServiceException(OperationCodes.OwnerCannotBeLocked,
                    $"Owner account: '{userId}' can not be locked.");
            }
            user.Lock();
            await _userRepository.UpdateAsync(user);
        }

        public async Task UnlockAsync(string userId)
        {
            var user = await _userRepository.GetOrFailAsync(userId);
            user.Unlock();
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteAsync(string userId, bool soft)
        {
            var user = await _userRepository.GetOrFailAsync(userId);
            if(soft)
            {
                user.MarkAsDeleted();
                await _userRepository.UpdateAsync(user);

                return;
            }
            await _userRepository.DeleteAsync(userId);
        }
    }
}