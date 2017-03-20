using System;
using Collectively.Common.Domain;
using Collectively.Common.Files;
using Collectively.Common.Services;
using Collectively.Common.Types;
using Collectively.Services.Users.Domain;
using Collectively.Services.Users.Queries;
using Collectively.Services.Users.Repositories;
using Collectively.Services.Users.Services;
using Machine.Specifications;
using Moq;
using It = Machine.Specifications.It;

namespace Collectively.Services.Users.Tests.Specs.Services
{
    public abstract class AvatarService_specs
    {
        protected static IAvatarService AvatarService;
        protected static Mock<IUserRepository> UserRepositoryMock;
        protected static Mock<IFileHandler> FileHandlerMock;
        protected static Mock<IImageService> ImageServiceMock;
        protected static Mock<IFileValidator> FileValidatorMock;
        protected static Exception Exception;
        protected static string UserId;
        protected static string Username;
        protected static string Email;
        protected static string ExternalId;
        protected static string Provider;
        protected static string Password;
        protected static string Hash;
        protected static string Role;
        protected static User User;
        protected static File File;

        protected static void Initialize()
        {
            UserId = "userId";
            Username = "Test-user_1";
            Email = "email@email.com";
            ExternalId = "externalId";
            Provider = Providers.Collectively;
            Password = "password";
            Hash = "hash";
            Role = Roles.User;
            FileHandlerMock = new Mock<IFileHandler>();
            ImageServiceMock = new Mock<IImageService>();
            FileValidatorMock = new Mock<IFileValidator>();
            UserRepositoryMock = new Mock<IUserRepository>();
            File = File.Create("avatar.jpg", "image/jpeg", new byte[] { 0x1 });
            AvatarService = new AvatarService(UserRepositoryMock.Object, FileHandlerMock.Object,
                ImageServiceMock.Object, FileValidatorMock.Object);
            User = new User(UserId, Email, Role, Provider);
        }
    }

    [Subject("UserService ChangeAvatarAsync")]
    public class When_changing_avatar_async : AvatarService_specs
    {
        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(User);
        };

        Because of = () => AvatarService.AddOrUpdateAsync(UserId, File);

        It should_call_update_async =
            () => UserRepositoryMock.Verify(x => x.UpdateAsync(Moq.It.Is<User>(u =>
                u.UserId == UserId
                && u.Avatar != null)), Times.Once);
    }

    [Subject("UserService ChangeAvatarAsync")]
    public class When_changing_avatar_async_and_user_do_not_exist : AvatarService_specs
    {
        protected static string NewUrl = "url";

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(null);
        };

        Because of = () => Exception = Catch.Exception(
            () => AvatarService.AddOrUpdateAsync(UserId, File).Await());

        It should_not_call_update_async =
            () => UserRepositoryMock.Verify(x => x.UpdateAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_service_exception =
            () => Exception.ShouldBeOfExactType<ServiceException>();
        It should_throw_exception_with_user_not_found_code = () =>
        {
            var exception = Exception as ServiceException;
            exception.Code.ShouldEqual(OperationCodes.UserNotFound);
        };
    }
}