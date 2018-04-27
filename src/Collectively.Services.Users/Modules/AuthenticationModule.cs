using System;
using System.Threading.Tasks;
using Collectively.Common.Security;
using Collectively.Common.Types;
using Collectively.Messages.Commands;
using Collectively.Messages.Commands.Users;
using Collectively.Services.Users.Services;
using Nancy;

namespace Collectively.Services.Users.Modules
{
    public class AuthenticationModule : ModuleBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserService _userService;
        private readonly IJwtTokenHandler _jwtTokenHandler;

        public AuthenticationModule(IServiceAuthenticatorHost serviceAuthenticatorHost,
            IAuthenticationService authenticationService,
            IUserService userService,
            IJwtTokenHandler jwtTokenHandler, 
            ICommandHandler<SignIn> signInHandler,
            ICommandHandler<RefreshUserSession> refreshSessionHandler) 
            : base(requireAuthentication: false)
        {
            _authenticationService = authenticationService;
            _userService = userService;
            _jwtTokenHandler = jwtTokenHandler;

            Post("authenticate", args => 
            {
                var credentials = BindRequest<Credentials>();
                var token = serviceAuthenticatorHost.CreateToken(credentials);
                if (token.HasNoValue)
                {
                    return HttpStatusCode.Unauthorized;
                }
                
                return token.Value;
            });

            Post("sign-in", async args =>
            {
                var command = BindRequest<SignIn>();
                if (command.SessionId == Guid.Empty)
                {
                    command.SessionId = Guid.NewGuid();
                }
                if (command.Request == null)
                {
                    command.Request = Messages.Commands.Request.New<SignIn>(Guid.NewGuid());
                }
                await signInHandler.HandleAsync(command);
                var session = await HandleSessionAsync(command.SessionId);
                if (session.HasNoValue)
                {
                    return HttpStatusCode.Unauthorized;
                }

                return session.Value;
            });

            Post("sessions", async args =>
            {
                var command = BindRequest<RefreshUserSession>();
                await refreshSessionHandler.HandleAsync(command);
                var session = await HandleSessionAsync(command.NewSessionId);
                if (session.HasNoValue)
                {
                    return HttpStatusCode.Forbidden;
                }

                return session.Value;
            });
        }

        private async Task<Maybe<JwtSession>> HandleSessionAsync(Guid sessionId) 
        {
            var session = await _authenticationService.GetSessionAsync(sessionId);
            if (session.HasNoValue)
            {
                return null;
            }
            var user = await _userService.GetAsync(session.Value.UserId);
            var token = _jwtTokenHandler.Create(user.Value.UserId, 
                user.Value.Role, state: user.Value.State);

            return new JwtSession
            {
                Token = token.Value.Token,
                Expires = token.Value.Expires,
                SessionId = session.Value.Id,
                Key = session.Value.Key
            };
        }        
    }
}