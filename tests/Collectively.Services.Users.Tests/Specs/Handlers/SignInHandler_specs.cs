using System;
using Collectively.Messages.Commands;
using Collectively.Common.Domain;
using Collectively.Common.Services;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Handlers;
using Collectively.Services.Users.Services;
using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using Machine.Specifications;
using Moq;
using RawRabbit;
using It = Machine.Specifications.It;
using Collectively.Common.Files;
using RawRabbit.Pipe;
using System.Threading;

namespace Collectively.Services.Users.Tests.Specs.Handlers
{
    public class SignInHandler_specs
    {
        protected static SignInHandler SignInHandler;
        protected static IHandler Handler;
        protected static Mock<IBusClient> BusClientMock;
        protected static Mock<IUserService> UserServiceMock;
        protected static Mock<IFacebookService> FacebookServiceMock;
        protected static Mock<IAuthenticationService> AuthenticationServiceMock;
        protected static Mock<IExceptionHandler> ExceptionHandlerMock;
        protected static Mock<IAvatarService> AvatarServiceMock;
        protected static Mock<IFileResolver> FileResolverMock;
        protected static Mock<IResourceFactory> ResourceFactoryMock;
        protected static SignIn Command;
        protected static User User;

        protected static void Initialize()
        {
            ExceptionHandlerMock = new Mock<IExceptionHandler>();
            Handler = new Handler(ExceptionHandlerMock.Object);
            BusClientMock = new Mock<IBusClient>();
            UserServiceMock = new Mock<IUserService>();
            FacebookServiceMock = new Mock<IFacebookService>();
            AuthenticationServiceMock = new Mock<IAuthenticationService>();
            AvatarServiceMock = new Mock<IAvatarService>();
            FileResolverMock = new Mock<IFileResolver>();
            ResourceFactoryMock = new Mock<IResourceFactory>();

            SignInHandler = new SignInHandler(Handler, BusClientMock.Object,
                UserServiceMock.Object, FacebookServiceMock.Object,
                AuthenticationServiceMock.Object, AvatarServiceMock.Object,
                FileResolverMock.Object, ResourceFactoryMock.Object);

            Command = new SignIn
            {
                Request = new Request
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    Culture = "en-US",
                    Name = "name",
                    Origin = "collectively",
                    Resource = "resource"
                },
                AccessToken = "token",
                Email = "email@email.com",
                IpAddress = "ip",
                Password = "password",
                Provider = "collectively",
                SessionId = Guid.NewGuid(),
                UserAgent = "user-agent"
            };

            User = new User("userId", "email@email.com", "role", "provider");

            UserServiceMock
                .Setup(x => x.GetByEmailAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .ReturnsAsync(User);
        }
    }

    [Subject("SignInHandler HandleAsync")]
    public class when_handle_async_sign_in_with_collectively : SignInHandler_specs
    {
        Establish context = () => Initialize();

        Because of = () => SignInHandler.HandleAsync(Command).Await();

        It should_call_sign_in_async = () => AuthenticationServiceMock.Verify(x => x.SignInAsync(
                Command.SessionId, Command.Email, Command.Password, Command.IpAddress, Command.UserAgent),
            Times.Once);

        It should_publish_user_signed_in_event = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.Is<SignedIn>(m => m.RequestId == Command.Request.Id
                                            && m.UserId == User.UserId
                                            && m.Email == User.Email
                                            && m.Name == User.Name
                                            && m.Provider == User.Provider),
            Moq.It.IsAny<Action<IPipeContext>>(),
            Moq.It.IsAny<CancellationToken>()),
            Times.Once);

        It should_not_publish_user_sign_in_rejected = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.IsAny<SignInRejected>(),
            Moq.It.IsAny<Action<IPipeContext>>(),
            Moq.It.IsAny<CancellationToken>()),
            Times.Never);
    }


    [Subject("SignInHandler HandleAsync")]
    public class when_handle_async_sign_in_with_collectively_and_it_throws_custom_exception : SignInHandler_specs
    {
        protected static string ErrorCode = "Error";

        Establish context = () =>
        {
            Initialize();
            AuthenticationServiceMock.Setup(x => x.SignInAsync(
                    Moq.It.IsAny<Guid>(), Moq.It.IsAny<string>(),
                    Moq.It.IsAny<string>(), Moq.It.IsAny<string>(),
                    Moq.It.IsAny<string>()))
                .Throws(new ServiceException(ErrorCode));
        };

        Because of = () => SignInHandler.HandleAsync(Command).Await();

        It should_call_sign_in_async = () => AuthenticationServiceMock.Verify(x => x.SignInAsync(
                Command.SessionId, Command.Email, Command.Password, Command.IpAddress, Command.UserAgent),
            Times.Once);

        It should_not_publish_user_signed_in_event = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.IsAny<SignedIn>(),
            Moq.It.IsAny<Action<IPipeContext>>(),
            Moq.It.IsAny<CancellationToken>()),
            Times.Never);

        It should_publish_user_sign_in_rejected = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.Is<SignInRejected>(m => m.RequestId == Command.Request.Id
                                                && m.Code == ErrorCode
                                                && m.Provider == Command.Provider),
            Moq.It.IsAny<Action<IPipeContext>>(),
            Moq.It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Subject("SignInHandler HandleAsync")]
    public class when_handle_async_sign_in_with_collectively_and_it_throws_exception : SignInHandler_specs
    {
        protected static string ErrorCode = "Error";

        Establish context = () =>
        {
            Initialize();
            AuthenticationServiceMock.Setup(x => x.SignInAsync(
                    Moq.It.IsAny<Guid>(), Moq.It.IsAny<string>(),
                    Moq.It.IsAny<string>(), Moq.It.IsAny<string>(),
                    Moq.It.IsAny<string>()))
                .Throws<Exception>();
        };

        Because of = () => SignInHandler.HandleAsync(Command).Await();

        It should_call_sign_in_async = () => AuthenticationServiceMock.Verify(x => x.SignInAsync(
                Command.SessionId, Command.Email, Command.Password, Command.IpAddress, Command.UserAgent),
            Times.Once);

        It should_not_publish_user_signed_in_event = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.IsAny<SignedIn>(),
            Moq.It.IsAny<Action<IPipeContext>>(),
            Moq.It.IsAny<CancellationToken>()),
            Times.Never);

        It should_publish_user_sign_in_rejected = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.Is<SignInRejected>(m => m.RequestId == Command.Request.Id
                                                && m.Code == OperationCodes.Error
                                                && m.Provider == Command.Provider),
            Moq.It.IsAny<Action<IPipeContext>>(),
            Moq.It.IsAny<CancellationToken>()),
            Times.Once);
    }

    //TODO write facebook sign in specs
}