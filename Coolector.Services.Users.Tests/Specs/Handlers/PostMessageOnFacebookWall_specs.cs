using System;
using Coolector.Common.Commands;
using Coolector.Common.Events.Facebook;
using Coolector.Common.Services;
using Coolector.Services.Users.Handlers;
using Coolector.Services.Users.Services;
using Coolector.Services.Users.Shared;
using Coolector.Services.Users.Shared.Commands.Facebook;
using Machine.Specifications;
using Moq;
using RawRabbit;
using RawRabbit.Configuration.Publish;
using It = Machine.Specifications.It;

namespace Coolector.Services.Users.Tests.Specs.Handlers
{
    public abstract class PostMessageOnFacebookWall_specs
    {
        protected static PostMessageOnFacebookWallHandler PostMessageOnFacebookWallHandler;
        protected static IHandler Handler;
        protected static Mock<IBusClient> BusClientMock;
        protected static Mock<IFacebookService> FacebookServiceMock;
        protected static PostMessageOnFacebookWall Command;

        protected static void Initialize()
        {
            Handler = new Handler();
            BusClientMock = new Mock<IBusClient>();
            FacebookServiceMock = new Mock<IFacebookService>();
            PostMessageOnFacebookWallHandler = new PostMessageOnFacebookWallHandler(Handler,
                BusClientMock.Object, FacebookServiceMock.Object);

            Command = new PostMessageOnFacebookWall
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
                    Origin = "coolector",
                    Resource = "resource"
                }
            };
        }
    }

    [Subject("PostMessageOnFacebookWallHandler HandleAsync")]
    public class When_handle_async_post_message_on_facebook : PostMessageOnFacebookWall_specs
    {
        Establish context = () => Initialize();

        Because of = () => PostMessageOnFacebookWallHandler.HandleAsync(Command).Await();

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
                Moq.It.IsAny<PostMessageOnFacebookWallRejected>(),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Never);

    }

    [Subject("PostMessageOnFacebookWallHandler HandleAsync")]
    public class When_handle_async_post_message_on_facebook_and_it_fails : PostMessageOnFacebookWall_specs
    {
        Establish context = () =>
        {
            Initialize();
            FacebookServiceMock.Setup(x => x.PostOnWallAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Throws<Exception>();
        };

        Because of = () => PostMessageOnFacebookWallHandler.HandleAsync(Command).Await();

        It should_call_post_on_wall_async = () => FacebookServiceMock.Verify(x => x.PostOnWallAsync(
                Command.AccessToken, Command.Message),
            Times.Once);

        It should_publish_message_posted_event = () => BusClientMock.Verify(x => x.PublishAsync(
                Moq.It.IsAny<MessageOnFacebookWallPosted>(),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Never);

        It should_not_publish_post_message_rejected = () => BusClientMock.Verify(x => x.PublishAsync(
                Moq.It.Is<PostMessageOnFacebookWallRejected>(m => m.RequestId == Command.Request.Id
                                                                  && m.UserId == Command.UserId
                                                                  && m.Code == OperationCodes.Error
                                                                  && m.Message == Command.Message),
                Moq.It.IsAny<Guid>(),
                Moq.It.IsAny<Action<IPublishConfigurationBuilder>>()),
            Times.Once);

    }
}