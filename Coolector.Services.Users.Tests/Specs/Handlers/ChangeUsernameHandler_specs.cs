using System;
using System.Threading.Tasks;
using Coolector.Common.Commands;
using Coolector.Common.Domain;
using Coolector.Common.Services;
using Coolector.Services.Users.Domain;
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
    public class ChangeUsernameHandler_specs
    {
        protected static ChangeUserNameHandler ChangeUserNameHandler;
        protected static IHandler Handler;
        protected static Mock<IBusClient> BusClientMock;
        protected static Mock<IUserService> UserServiceMock;

        protected static ChangeUserName Command;
        protected static User User;

        protected static void Initialize()
        {
            Handler = new Handler();
            BusClientMock = new Mock<IBusClient>();
            UserServiceMock = new Mock<IUserService>();

            ChangeUserNameHandler = new ChangeUserNameHandler(Handler, 
                BusClientMock.Object, UserServiceMock.Object);

            Command = new ChangeUserName
            {
                Name = "newName",
                UserId = "userId",
                Request = new Request
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    Culture = "en-US",
                    Name = "name",
                    Origin = "coolector",
                    Resource = "resource"
                }
            };
            User = new User("userId", "email@email.com", Roles.User, "coolector");
            UserServiceMock
                .Setup(x => x.ChangeNameAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Callback(() =>
                {
                    User.SetName(Command.Name);
                    User.Activate();
                })
                .Returns(Task.CompletedTask);
            UserServiceMock
                .Setup(x => x.GetAsync(Command.UserId))
                .ReturnsAsync(User);
        }
    }

    [Subject("ChangeUserNameHandler HandleAsync")]
    public class When_handle_async_change_username_command : ChangeUsernameHandler_specs
    {
        Establish context = () => Initialize();

        Because of = () => ChangeUserNameHandler.HandleAsync(Command).Await();

        It should_call_change_name_async =
            () => UserServiceMock.Verify(x => x.ChangeNameAsync(Command.UserId, Command.Name), Times.Once);
        It should_call_get_user_async =
            () => UserServiceMock.Verify(x => x.GetAsync(Command.UserId), Times.Once);

        It should_publish_username_changed_event =
            () => BusClientMock.Verify(x => x.PublishAsync(Moq.It.Is<UserNameChanged>(m =>
                    m.RequestId == Command.Request.Id
                    && m.UserId == Command.UserId
                    && m.NewName == Command.Name
                    && m.State == States.Active),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()), Times.Once);

    }

    [Subject("ChangeUserNameHandler HandleAsync")]
    public class When_handle_async_change_username_and_it_throws_custom_exception : ChangeUsernameHandler_specs
    {
        protected static string ErrorCode = "Error";

        Establish context = () =>
        {
            Initialize();
            UserServiceMock
                .Setup(x => x.ChangeNameAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Throws(new ServiceException(ErrorCode));
        };

        Because of = () => ChangeUserNameHandler.HandleAsync(Command).Await();

        It should_call_change_name_async =
            () => UserServiceMock.Verify(x => x.ChangeNameAsync(Command.UserId, Command.Name), Times.Once);
        It should_not_call_get_user_async =
            () => UserServiceMock.Verify(x => x.GetAsync(Command.UserId), Times.Never);
        It should_not_publish_username_changed_event =
            () => BusClientMock.Verify(x => x.PublishAsync(Moq.It.IsAny<UserNameChanged>(),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()), Times.Never);
        It should_publish_change_username_rejected_event =
            () => BusClientMock.Verify(x => x.PublishAsync(Moq.It.Is<ChangeUsernameRejected>(m =>
                    m.RequestId == Command.Request.Id
                    && m.RejectedUsername == Command.Name
                    && m.UserId == Command.UserId
                    && m.Code == ErrorCode),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()), Times.Once);

    }

    [Subject("ChangeUserNameHandler HandleAsync")]
    public class When_handle_async_change_username_and_it_throws_exception : ChangeUsernameHandler_specs
    {
        protected static string ErrorCode = "Error";

        Establish context = () =>
        {
            Initialize();
            UserServiceMock
                .Setup(x => x.ChangeNameAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Throws<Exception>();
        };

        Because of = () => ChangeUserNameHandler.HandleAsync(Command).Await();

        It should_call_change_name_async =
            () => UserServiceMock.Verify(x => x.ChangeNameAsync(Command.UserId, Command.Name), Times.Once);
        It should_not_call_get_user_async =
            () => UserServiceMock.Verify(x => x.GetAsync(Command.UserId), Times.Never);
        It should_not_publish_username_changed_event =
            () => BusClientMock.Verify(x => x.PublishAsync(Moq.It.IsAny<UserNameChanged>(),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()), Times.Never);
        It should_publish_change_username_rejected_event =
            () => BusClientMock.Verify(x => x.PublishAsync(Moq.It.Is<ChangeUsernameRejected>(m =>
                    m.RequestId == Command.Request.Id
                    && m.RejectedUsername == Command.Name
                    && m.UserId == Command.UserId
                    && m.Code == OperationCodes.Error),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()), Times.Once);

    }
}