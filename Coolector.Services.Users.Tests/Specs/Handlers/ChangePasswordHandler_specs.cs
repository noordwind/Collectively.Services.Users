using System;
using Coolector.Common.Commands;
using Coolector.Common.Commands.Users;
using Coolector.Common.Events.Users;
using Coolector.Common.Services;
using Coolector.Services.Users.Handlers;
using Coolector.Services.Users.Services;
using Machine.Specifications;
using Moq;
using RawRabbit;
using RawRabbit.Configuration.Publish;
using It = Machine.Specifications.It; 

namespace Coolector.Services.Users.Tests.Specs.Handlers
{
    public abstract class ChangePasswordHandler_specs
    {
        protected static ChangePasswordHandler ChangePasswordHandler;
        protected static IHandler Handler;
        protected static Mock<IBusClient> BusClientMock;
        protected static Mock<IPasswordService> PasswordServiceMock;

        protected static ChangePassword Command;

        protected static void Initialize()
        {
            Handler = new Handler();
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
                    Origin = "coolector",
                    Resource = "resource"
                }
            };
        }
    }

    [Subject("ChangePasswordHandler HandleAsync")]
    public class When_handle_async : ChangePasswordHandler_specs
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
            Moq.It.IsAny<Guid>(),
            Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()), Times.Once);


        It should_not_publish_change_password_rejected_event = () => BusClientMock.Verify(x => x.PublishAsync(
            Moq.It.IsAny<ChangePasswordRejected>(),
            Moq.It.IsAny<Guid>(),
            Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()), Times.Never);
    }
}