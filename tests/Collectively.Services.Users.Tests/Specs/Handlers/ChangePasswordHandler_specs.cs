using System;
using Collectively.Messages.Commands;
using Collectively.Common.Domain;
using Collectively.Common.Services;
using Collectively.Services.Users.Handlers;
using Collectively.Services.Users.Services;
using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using Machine.Specifications;
using Moq;
using RawRabbit;
using It = Machine.Specifications.It;
using RawRabbit.Pipe;
using System.Threading;

namespace Collectively.Services.Users.Tests.Specs.Handlers
{
    public abstract class ChangePasswordHandler_specs
    {
        protected static ChangePasswordHandler ChangePasswordHandler;
        protected static IHandler Handler;
        protected static Mock<IBusClient> BusClientMock;
        protected static Mock<IPasswordService> PasswordServiceMock;
        protected static Mock<IExceptionHandler> ExceptionHandlerMock;
        protected static ChangePassword Command;

        protected static void Initialize()
        {
            ExceptionHandlerMock = new Mock<IExceptionHandler>();
            Handler = new Handler(ExceptionHandlerMock.Object);
            BusClientMock = new Mock<IBusClient>();
            PasswordServiceMock = new Mock<IPasswordService>();

            ChangePasswordHandler = new ChangePasswordHandler(Handler, 
                BusClientMock.Object, PasswordServiceMock.Object);

            Command = new ChangePassword
            {
                CurrentPassword = "current",
                NewPassword = "new",
                UserId = "userId",
                Request = new Request
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    Culture = "en-US",
                    Name = "name",
                    Origin = "collectively",
                    Resource = "resource"
                }
            };
        }
    }

    [Subject("ChangePasswordHandler HandleAsync")]
    public class when_handle_async_change_password_command : ChangePasswordHandler_specs
    {
        Establish context = () => Initialize();

        Because of = () => ChangePasswordHandler.HandleAsync(Command).Await();

        It should_call_change_async = () => PasswordServiceMock.Verify(x => x.ChangeAsync(
            Command.UserId,
            Command.CurrentPassword,
            Command.NewPassword), Times.Once);

        It should_publish_password_changed_event = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.Is<PasswordChanged>(e => e.RequestId == Command.Request.Id
                                            && e.UserId == Command.UserId),
                Moq.It.IsAny<Action<IPipeContext>>(),
                Moq.It.IsAny<CancellationToken>()), Times.Once);


        It should_not_publish_change_password_rejected_event = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.IsAny<ChangePasswordRejected>(),
            Moq.It.IsAny<Action<IPipeContext>>(),
            Moq.It.IsAny<CancellationToken>()), Times.Never);
    }

    [Subject("ChangePasswordHandler HandleAsync")]
    public class when_handle_async_change_password_command_and_it_fails : ChangePasswordHandler_specs
    {
        private Establish context = () =>
        {
            Initialize();
            PasswordServiceMock
                .Setup(x => x.ChangeAsync(Moq.It.IsAny<string>(),
                    Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Throws<Exception>();
        };

        Because of = () => ChangePasswordHandler.HandleAsync(Command).Await();

        It should_call_change_async = () => PasswordServiceMock.Verify(x => x.ChangeAsync(
            Command.UserId,
            Command.CurrentPassword,
            Command.NewPassword), Times.Once);

        It should_not_publish_password_changed_event = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.IsAny<PasswordChanged>(),
            Moq.It.IsAny<Action<IPipeContext>>(),
            Moq.It.IsAny<CancellationToken>()), Times.Never);


        It should_publish_change_password_rejected_event = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.Is<ChangePasswordRejected>(m => m.RequestId == Command.Request.Id
                                                   && m.UserId == Command.UserId
                                                   && m.Code == OperationCodes.Error),
            Moq.It.IsAny<Action<IPipeContext>>(),
            Moq.It.IsAny<CancellationToken>()), Times.Once);
    }

    [Subject("ChangePasswordHandler HandleAsync")]
    public class when_handle_async_change_password_command_and_it_throws_custom_exception : ChangePasswordHandler_specs
    {
        protected static string ErrorCode = "Error";

        private Establish context = () =>
        {
            Initialize();
            PasswordServiceMock
                .Setup(x => x.ChangeAsync(Moq.It.IsAny<string>(),
                    Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Throws(new ServiceException(ErrorCode));
        };

        Because of = () => ChangePasswordHandler.HandleAsync(Command).Await();

        It should_call_change_async = () => PasswordServiceMock.Verify(x => x.ChangeAsync(
            Command.UserId,
            Command.CurrentPassword,
            Command.NewPassword), Times.Once);

        It should_not_publish_password_changed_event = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.IsAny<PasswordChanged>(),
            Moq.It.IsAny<Action<IPipeContext>>(),
            Moq.It.IsAny<CancellationToken>()), Times.Never);


        It should_publish_change_password_rejected_event = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.Is<ChangePasswordRejected>(m => m.RequestId == Command.Request.Id
                                                   && m.UserId == Command.UserId
                                                   && m.Code == ErrorCode),
            Moq.It.IsAny<Action<IPipeContext>>(),
            Moq.It.IsAny<CancellationToken>()), Times.Once);
    }
}