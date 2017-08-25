using System;
using System.Threading.Tasks;
using Collectively.Common.Domain;
using Collectively.Common.Services;
using Collectively.Common.Types;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Repositories;


namespace Collectively.Services.Users.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IFacebookService _facebookService;
        private readonly IEncrypter _encrypter;

        public AuthenticationService(IUserRepository userRepository,
            IUserSessionRepository userSessionRepository,
            IFacebookService facebookService,
            IEncrypter encrypter)
        {
            _userRepository = userRepository;
            _userSessionRepository = userSessionRepository;
            _facebookService = facebookService;
            _encrypter = encrypter;
        }

        public async Task<Maybe<UserSession>> GetSessionAsync(Guid id)
            => await _userSessionRepository.GetByIdAsync(id);

        public async Task SignInAsync(Guid sessionId, string email, string password,
            string ipAddress = null, string userAgent = null)
        {
            var user = await _userRepository.GetByEmailAsync(email, Providers.Collectively);
            if (user.HasNoValue)
            {
                throw new ServiceException(OperationCodes.UserNotFound,
                    $"User with email '{email}' has not been found.");
            }
            if (user.Value.State != States.Active && user.Value.State != States.Unconfirmed)
            {
                throw new ServiceException(OperationCodes.InactiveUser,
                    $"User '{user.Value.Id}' is not active.");
            }
            if (!user.Value.ValidatePassword(password, _encrypter))
            {
                throw new ServiceException(OperationCodes.InvalidCredentials,
                    "Invalid credentials.");
            }
            await CreateSessionAsync(sessionId, user.Value);
        }

        public async Task SignInViaFacebookAsync(Guid sessionId, string accessToken,
            string ipAddress = null, string userAgent = null)
        {
            var facebookUser = await _facebookService.GetUserAsync(accessToken);
            if (facebookUser.HasNoValue)
            {
                throw new ServiceException(OperationCodes.UserNotFound,
                    $"Facebook user has not been found for given access token.");
            }
            var user = await _userRepository.GetByExternalUserIdAsync(facebookUser.Value.Id);
            if (user.HasNoValue)
            {
                throw new ServiceException(OperationCodes.UserNotFound,
                    $"User with Facebook external id: " +
                    $"'{facebookUser.Value.Id}' has not been found.");
            }
            if (user.Value.State != States.Active && user.Value.State != States.Incomplete)
            {
                throw new ServiceException(OperationCodes.InactiveUser,
                    $"User '{user.Value.Id}' is neither active nor incomplete.");
            }
            await CreateSessionAsync(sessionId, user.Value);
        }

        public async Task SignOutAsync(Guid sessionId, string userId)
        {
            var user = await _userRepository.GetByUserIdAsync(userId);
            if (user.HasNoValue)
            {
                throw new ServiceException(OperationCodes.UserNotFound,
                    $"User with id '{userId}' has not been found.");
            }

            var session = await _userSessionRepository.GetByIdAsync(sessionId);
            if (session.HasNoValue)
            {
                throw new ServiceException(OperationCodes.SessionNotFound,
                    $"Session with id '{sessionId}' has not been found.");
            }
            session.Value.Destroy();
            await _userSessionRepository.UpdateAsync(session.Value);
        }

        public async Task CreateSessionAsync(Guid sessionId, string userId,
            string ipAddress = null,
            string userAgent = null)
        {
            var user = await _userRepository.GetByUserIdAsync(userId);
            if (user.HasNoValue)
            {
                throw new ServiceException(OperationCodes.UserNotFound,
                    $"User with id '{userId}' has not been found.");
            }
            await CreateSessionAsync(sessionId, user.Value);
        }

        private async Task CreateSessionAsync(Guid sessionId, User user,
            string ipAddress = null, string userAgent = null)
        {
            var session = new UserSession(sessionId, user.UserId,
                _encrypter.GetRandomSecureKey(), ipAddress, userAgent);
            await _userSessionRepository.AddAsync(session);
        }

        public async Task RefreshSessionAsync(Guid sessionId, string sessionKey,
            string ipAddress = null, string userAgent = null)
        {
            var parentSession = await _userSessionRepository.GetByIdAsync(sessionId);
            if (parentSession.HasNoValue)
            {
                throw new ServiceException(OperationCodes.SessionNotFound,
                    $"Session with id '{sessionId}' has not been found.");
            }
            if (parentSession.Value.Key != sessionKey)
            {
                throw new ServiceException(OperationCodes.InvalidSessionKey,
                    $"Invalid session key: '{sessionKey}'");
            }
            var newSession = parentSession.Value.Refresh(Guid.NewGuid(),
                _encrypter.GetRandomSecureKey(), sessionId, ipAddress, userAgent);
            await _userSessionRepository.UpdateAsync(parentSession.Value);
            await _userSessionRepository.AddAsync(newSession);
        }
    }
}