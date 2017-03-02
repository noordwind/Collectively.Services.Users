using System;
using Collectively.Messages.Commands;
using Collectively.Common.Domain;
using Collectively.Common.Services;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Handlers;
using Collectively.Services.Users.Services;

using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using Collectively.Messages.Commands.Mailing;
using Machine.Specifications;
using Moq;
using RawRabbit;
using RawRabbit.Configuration.Publish;
using It = Machine.Specifications.It;

namespace Collectively.Services.Users.Tests.Specs.Handlers
{
    public abstract class ResetPasswordHandler_specs
    {
        protected static ResetPasswordHandler ResetPasswordHandler;
        protected static IHandler Handler;
        protected static Mock<IBusClient> BusMock;
        protected static Mock<IPasswordService> PasswordServiceMock;
        protected static Mock<IOneTimeSecuredOperationService> OneTimeOperationServiceMock;
        protected static Mock<IExceptionHandler> ExceptionHandlerMock;

        protected static ResetPassword Command;
        protected static string Token;

        public static void Initialize()
        {
            ExceptionHandlerMock = new Mock<IExceptionHandler>();
            Handler = new Handler(ExceptionHandlerMock.Object);
            BusMock = new Mock<IBusClient>();
            PasswordServiceMock = new Mock<IPasswordService>();
            OneTimeOperationServiceMock = new Mock<IOneTimeSecuredOperationService>();

            ResetPasswordHandler = new ResetPasswordHandler(Handler, BusMock.Object,
                PasswordServiceMock.Object, OneTimeOperationServiceMock.Object);

            var email = "email@email.com";
            Command = new ResetPassword
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
                Email = email,
                Endpoint = "endpoint"
            };

            Token = "token";
            var oneTimeOperation = new OneTimeSecuredOperation(Guid.Empty, "type", 
                email, Token, DateTime.MaxValue);
            OneTimeOperationServiceMock.Setup(x => x.GetAsync(Moq.It.IsAny<Guid>()))
                .ReturnsAsync(oneTimeOperation);
        }
    }

    [Subject("ResetPasswordHandler HandleAsync")]
    public class When_handle_async_reset_password_command : ResetPasswordHandler_specs
    {
        Establish context = () => Initialize();

        Because of = () => ResetPasswordHandler.HandleAsync(Command).Await();

        It should_call_reset_async = () => PasswordServiceMock.Verify(x => x.ResetAsync(
            Moq.It.IsAny<Guid>(),
            Command.Email), Times.Once);

        It should_call_get_one_time_operation = ()
            => OneTimeOperationServiceMock.Verify(x => x.GetAsync(Moq.It.IsAny<Guid>()), Times.Once);

        It should_publish_send_reset_password_email_command = () => BusMock.Verify(x => x.PublishAsync(
                Moq.It.Is<SendResetPasswordEmailMessage>(c => c.Email == Command.Email
                                                              && c.Request.Id == Command.Request.Id
                                                              && c.Token == Token
                                                              && c.Endpoint == Command.Endpoint),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Once);

        It should_not_publish_send_reset_password_rejected = () => BusMock.Verify(x => x.PublishAsync(
                Moq.It.IsAny<ResetPasswordRejected>(),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Never);
    }

    [Subject("ResetPasswordHandler HandleAsync")]
    public class When_handle_async_reset_password_command_and_it_fails : ResetPasswordHandler_specs
    {
        Establish context = () =>
        {
            Initialize();
            PasswordServiceMock
                .Setup(x => x.ResetAsync(Moq.It.IsAny<Guid>(), Moq.It.IsAny<string>()))
                .Throws<Exception>();
        };

        Because of = () => ResetPasswordHandler.HandleAsync(Command).Await();

        It should_call_reset_async = () => PasswordServiceMock.Verify(x => x.ResetAsync(
            Moq.It.IsAny<Guid>(),
            Command.Email), Times.Once);

        It should_not_call_get_one_time_operation = ()
            => OneTimeOperationServiceMock.Verify(x => x.GetAsync(Moq.It.IsAny<Guid>()), Times.Never);

        It should_not_publish_send_reset_password_email_command = () => BusMock.Verify(x => x.PublishAsync(
                Moq.It.IsAny<SendResetPasswordEmailMessage>(),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Never);

        It should_publish_send_reset_password_rejected = () => BusMock.Verify(x => x.PublishAsync(
                Moq.It.Is<ResetPasswordRejected>(m => m.RequestId == Command.Request.Id
                                                      && m.Email == Command.Email 
                                                      && m.Code == OperationCodes.Error),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Once);
    }

    [Subject("ResetPasswordHandler HandleAsync")]
    public class When_handle_async_reset_password_command_and_it_throws_custom_exception : ResetPasswordHandler_specs
    {
        protected static string ErrorCode = "Error";

        Establish context = () =>
        {
            Initialize();
            PasswordServiceMock
                .Setup(x => x.ResetAsync(Moq.It.IsAny<Guid>(), Moq.It.IsAny<string>()))
                .Throws(new ServiceException(ErrorCode));
        };

        Because of = () => ResetPasswordHandler.HandleAsync(Command).Await();

        It should_call_reset_async = () => PasswordServiceMock.Verify(x => x.ResetAsync(
            Moq.It.IsAny<Guid>(),
            Command.Email), Times.Once);

        It should_not_call_get_one_time_operation = ()
            => OneTimeOperationServiceMock.Verify(x => x.GetAsync(Moq.It.IsAny<Guid>()), Times.Never);

        It should_not_publish_send_reset_password_email_command = () => BusMock.Verify(x => x.PublishAsync(
                Moq.It.IsAny<SendResetPasswordEmailMessage>(),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Never);

        It should_publish_send_reset_password_email_command = () => BusMock.Verify(x => x.PublishAsync(
                Moq.It.Is<ResetPasswordRejected>(m => m.RequestId == Command.Request.Id
                                                      && m.Email == Command.Email
                                                      && m.Code == ErrorCode),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Once);
    }
}