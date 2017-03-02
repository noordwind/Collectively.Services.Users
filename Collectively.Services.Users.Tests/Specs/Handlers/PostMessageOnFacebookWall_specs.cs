using System;
using Collectively.Messages.Commands;
using Collectively.Common.Services;
using Collectively.Services.Users.Handlers;
using Collectively.Services.Users.Services;
using Collectively.Messages.Commands.Users;
using Collectively.Messages.Events.Users;
using Machine.Specifications;
using Moq;
using RawRabbit;
using RawRabbit.Configuration.Publish;
using It = Machine.Specifications.It;

namespace Collectively.Services.Users.Tests.Specs.Handlers
{
    public abstract class PostMessageOnFacebookWall_specs
    {
        protected static PostOnFacebookWallHandler PostOnFacebookWallHandler;
        protected static IHandler Handler;
        protected static Mock<IBusClient> BusClientMock;
        protected static Mock<IFacebookService> FacebookServiceMock;
        protected static Mock<IExceptionHandler> ExceptionHandlerMock;
        protected static PostOnFacebookWall Command;

        protected static void Initialize()
        {
            ExceptionHandlerMock = new Mock<IExceptionHandler>();
            Handler = new Handler(ExceptionHandlerMock.Object);
            BusClientMock = new Mock<IBusClient>();
            FacebookServiceMock = new Mock<IFacebookService>();
            PostOnFacebookWallHandler = new PostOnFacebookWallHandler(Handler,
                BusClientMock.Object, FacebookServiceMock.Object);

            Command = new PostOnFacebookWall
            {
                AccessToken = "token",
                Message = "message",
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

    [Subject("PostOnFacebookWallHandler HandleAsync")]
    public class When_handle_async_post_message_on_facebook : PostMessageOnFacebookWall_specs
    {
        Establish context = () => Initialize();

        Because of = () => PostOnFacebookWallHandler.HandleAsync(Command).Await();

        It should_call_post_on_wall_async = () => FacebookServiceMock.Verify(x => x.PostOnWallAsync(
                Command.AccessToken, Command.Message),
            Times.Once);

        It should_publish_message_posted_event = () => BusClientMock.Verify(x => x.PublishAsync(
                Moq.It.Is<MessageOnFacebookWallPosted>(m => m.RequestId == Command.Request.Id
                                                            && m.UserId == Command.UserId
                                                            && m.Message == Command.Message),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Once);

        It should_not_publish_post_message_rejected = () => BusClientMock.Verify(x => x.PublishAsync(
                Moq.It.IsAny<PostOnFacebookWallRejected>(),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Never);

    }

    [Subject("PostOnFacebookWallHandler HandleAsync")]
    public class When_handle_async_post_message_on_facebook_and_it_fails : PostMessageOnFacebookWall_specs
    {
        Establish context = () =>
        {
            Initialize();
            FacebookServiceMock.Setup(x => x.PostOnWallAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Throws<Exception>();
        };

        Because of = () => PostOnFacebookWallHandler.HandleAsync(Command).Await();

        It should_call_post_on_wall_async = () => FacebookServiceMock.Verify(x => x.PostOnWallAsync(
                Command.AccessToken, Command.Message),
            Times.Once);

        It should_publish_message_posted_event = () => BusClientMock.Verify(x => x.PublishAsync(
                Moq.It.IsAny<MessageOnFacebookWallPosted>(),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Never);

        It should_not_publish_post_message_rejected = () => BusClientMock.Verify(x => x.PublishAsync(
                Moq.It.Is<PostOnFacebookWallRejected>(m => m.RequestId == Command.Request.Id
                                                                  && m.UserId == Command.UserId
                                                                  && m.Code == OperationCodes.Error
                                                                  && m.Message == Command.Message),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Once);

    }
}