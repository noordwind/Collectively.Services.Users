using System;
using System.Threading.Tasks;
using Collectively.Messages.Commands;
using Collectively.Common.Services;
using Collectively.Common.Types;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Services;
using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using NLog;
using RawRabbit;
using Collectively.Common.Files;
using System.IO;

namespace Collectively.Services.Users.Handlers
{
    public class SignInHandler : ICommandHandler<SignIn>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IUserService _userService;
        private readonly IFacebookService _facebookService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAvatarService _avatarService;
        private readonly IFileResolver _fileResolver;
        private readonly IResourceFactory _resourceFactory;

        public SignInHandler(IHandler handler,
            IBusClient bus,
            IUserService userService,
            IFacebookService facebookService,
            IAuthenticationService authenticationService, 
            IAvatarService avatarService,
            IFileResolver fileResolver,
            IResourceFactory resourceFactory)
        {
            _handler = handler;
            _bus = bus;
            _userService = userService;
            _facebookService = facebookService;
            _authenticationService = authenticationService;
            _avatarService = avatarService;
            _fileResolver = fileResolver;
            _resourceFactory = resourceFactory;
        }

        public async Task HandleAsync(SignIn command)
        {
            Maybe<User> user = null;
            await _handler
                .Run(async () =>
                {
                    switch (command.Provider?.ToLowerInvariant())
                    {
                        case "collectively":
                            user = await HandleDefaultSignInAsync(command);
                            break;
                        case "facebook":
                            user = await HandleFacebookSignInAsync(command);
                            break;
                        default:
                            throw new ArgumentException($"Invalid provider: {command.Provider}", nameof(command.Provider));
                    }
                })
                .OnSuccess(async () => await _bus.PublishAsync(new SignedIn(command.Request.Id,
                    user.Value.UserId, user.Value.Email, user.Value.Name, user.Value.Provider)))
                .OnCustomError(async ex => await _bus.PublishAsync(new SignInRejected(command.Request.Id,
                    null, ex.Code, ex.Message, command.Provider)))
                .OnError(async (ex, logger) =>
                {
                    Logger.Error(ex, "Error occured while signing in");
                    await _bus.PublishAsync(new SignInRejected(command.Request.Id,
                        null, OperationCodes.Error, ex.Message, command.Provider));
                })
                .ExecuteAsync();
        }

        private async Task<Maybe<User>> HandleDefaultSignInAsync(SignIn command)
        {
            await _authenticationService.SignInAsync(command.SessionId,
                command.Email, command.Password, command.IpAddress, command.UserAgent);

            return await _userService.GetByEmailAsync(command.Email, command.Provider);
        }

        private async Task<Maybe<User>> HandleFacebookSignInAsync(SignIn command)
        {
            var facebookUser = await _facebookService.GetUserAsync(command.AccessToken);
            if (facebookUser.HasNoValue)
                return new Maybe<User>();

            var externalUserId = facebookUser.Value.Id;
            var user = await _userService.GetByExternalUserIdAsync(externalUserId);
            if (user.HasValue)
            {
                await _authenticationService.SignInViaFacebookAsync(command.SessionId,
                    command.AccessToken, command.IpAddress, command.UserAgent);

                return user;
            }

            var userId = Guid.NewGuid().ToString("N");
            await _userService.SignUpAsync(userId, facebookUser.Value.Email,
                Roles.User, Providers.Facebook, externalUserId: externalUserId);

            Logger.Info($"Created new user with id: '{userId}' using Facebook user id: '{externalUserId}'");
            await TryUploadFacebookAvatar(userId, externalUserId);
            user = await _userService.GetByExternalUserIdAsync(externalUserId);
            var resource = _resourceFactory.Resolve<SignedUp>(userId);
            await _bus.PublishAsync(new SignedUp(command.Request.Id, resource, userId, 
                user.Value.Provider));

            await _authenticationService.SignInViaFacebookAsync(command.SessionId, command.AccessToken,
                command.IpAddress, command.UserAgent);

            return await _userService.GetByExternalUserIdAsync(externalUserId);
        }

        private async Task TryUploadFacebookAvatar(string userId, string externalUserId)
        {
            try
            {
                var avatarUrl = _facebookService.GetAvatarUrl(externalUserId);
                var avatar = await _fileResolver.FromUrlAsync(avatarUrl);
                if(avatar.HasNoValue)
                {
                    return;
                }
                var file = Common.Files.File.Empty;
                using (var stream = new MemoryStream())
                {
                    await avatar.Value.CopyToAsync(stream);
                    file = Common.Files.File.Create($"{userId}.jpg", "image/jpeg", stream.ToArray());
                }
                await _avatarService.AddOrUpdateAsync(userId, file);
            }
            catch(Exception ex)
            {
                Logger.Error(ex, $"There was an error when trying to upload the avatar from Facebook for user: {externalUserId}.");
            }
        }
    }
}