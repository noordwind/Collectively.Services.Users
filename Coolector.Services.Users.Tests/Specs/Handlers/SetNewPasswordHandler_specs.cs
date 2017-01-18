using System;
using Coolector.Common.Commands;
using Coolector.Common.Domain;
using Coolector.Common.Services;
using Coolector.Services.Users.Handlers;
using Coolector.Services.Users.Services;
using Coolector.Services.Users.Shared;
using Coolector.Services.Users.Shared.Commands;
using Coolector.Services.Users.Shared.Events;
using Machine.Specifications;
using Moq;
using RawRabbit;
using RawRabbit.Configuration.Publish;
using It = Machine.Specifications.It;

namespace Coolector.Services.Users.Tests.Specs.Handlers
{
    public class SetNewPasswordHandler_specs
    {
        protected static SetNewPasswordHandler SetNewPasswordHandler;
        protected static IHandler Handler;
        protected static Mock<IBusClient> BusClientMock;
        protected static Mock<IPasswordService> PasswordServiceMock;
        protected static Mock<IExceptionHandler> ExceptionHandlerMock;

        protected static SetNewPassword Command;

        protected static void Initialize()
        {
            ExceptionHandlerMock = new Mock<IExceptionHandler>();
            Handler = new Handler(ExceptionHandlerMock.Object);
            BusClientMock = new Mock<IBusClient>();
            PasswordServiceMock = new Mock<IPasswordService>();
            
            SetNewPasswordHandler = new SetNewPasswordHandler(Handler, 
                BusClientMock.Object, PasswordServiceMock.Object);

            Command = new SetNewPassword
            {
                Request = new Request
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    Culture = "en-US",
                    Name = "name",
                    Origin = "coolector",
                    Resource = "resource"
                },
                Email = "email",
                Password = "password",
                Token = "token"
            };
        }
    }

    [Subject("SetNewPasswordHandler HandleAsync")]
    public class When_handle_async_set_new_password : SetNewPasswordHandler_specs
    {
        Establish context = () => Initialize();

        Because of = () => SetNewPasswordHandler.HandleAsync(Command).Await();

        It should_call_set_new_async = () => PasswordServiceMock.Verify(x => x.SetNewAsync(
            Command.Email, Command.Token, Command.Password), Times.Once);

        It should_publish_new_password_set_event = () => BusClientMock.Verify(x => x.PublishAsync(
                Moq.It.Is<NewPasswordSet>(m => m.RequestId == Command.Request.Id
                                               && m.Email == Command.Email),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Once);

        It should_not_publish_set_new_password_rejected = () => BusClientMock.Verify(x => x.PublishAsync(
                Moq.It.IsAny<SetNewPasswordRejected>(),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Never);
    }


    [Subject("SetNewPasswordHandler HandleAsync")]
    public class When_handle_async_set_new_password_and_it_throws_custom_error : SetNewPasswordHandler_specs
    {
        protected static string ErrorCode = "Error";

        Establish context = () =>
        {
            Initialize();
            PasswordServiceMock.Setup(x => x.SetNewAsync(
                    Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Throws(new ServiceException(ErrorCode));
        };

        Because of = () => SetNewPasswordHandler.HandleAsync(Command).Await();

        It should_call_set_new_async = () => PasswordServiceMock.Verify(x => x.SetNewAsync(
            Command.Email, Command.Token, Command.Password), Times.Once);

        It should_not_publish_new_password_set_event = () => BusClientMock.Verify(x => x.PublishAsync(
                Moq.It.IsAny<NewPasswordSet>(),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Never);

        It should_publish_set_new_password_rejected = () => BusClientMock.Verify(x => x.PublishAsync(
                Moq.It.Is<SetNewPasswordRejected>(m => m.RequestId == Command.Request.Id
                                                       && m.Code == ErrorCode
                                                       && m.Email == Command.Email),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Once);
    }

    [Subject("SetNewPasswordHandler HandleAsync")]
    public class When_handle_async_set_new_password_and_it_fails : SetNewPasswordHandler_specs
    {
        Establish context = () =>
        {
            Initialize();
            PasswordServiceMock.Setup(x => x.SetNewAsync(
                    Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Throws<Exception>();
        };

        Because of = () => SetNewPasswordHandler.HandleAsync(Command).Await();

        It should_call_set_new_async = () => PasswordServiceMock.Verify(x => x.SetNewAsync(
            Command.Email, Command.Token, Command.Password), Times.Once);

        It should_not_publish_new_password_set_event = () => BusClientMock.Verify(x => x.PublishAsync(
                Moq.It.IsAny<NewPasswordSet>(),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Never);

        It should_publish_set_new_password_rejected = () => BusClientMock.Verify(x => x.PublishAsync(
                Moq.It.Is<SetNewPasswordRejected>(m => m.RequestId == Command.Request.Id
                                                       && m.Code == OperationCodes.Error
                                                       && m.Email == Command.Email),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Once);
    }
}