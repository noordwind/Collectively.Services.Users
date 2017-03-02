using System;
using Collectively.Common.Domain;
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
    public abstract class UserService_specs
    {
        protected static IUserService UserService;
        protected static Mock<IUserRepository> UserRepositoryMock;
        protected static Mock<IEncrypter> EncrypterMock;

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
        protected static BrowseUsers Query;
        protected static PagedResult<User> Users;

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
            UserRepositoryMock = new Mock<IUserRepository>();
            EncrypterMock = new Mock<IEncrypter>();
            UserService = new UserService(UserRepositoryMock.Object, EncrypterMock.Object);
            User = new User(UserId, Email, Role, Provider);
            Query = new BrowseUsers();
            Users = PagedResult<User>.Create(new []{User}, 1, 1, 1, 1);
            EncrypterMock.Setup(x => x.GetSalt(Moq.It.IsAny<string>())).Returns("salt");
            EncrypterMock
                .Setup(x => x.GetHash(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns(Hash);
        }
    }

    [Subject("UserService IsNameAvailableAsync")]
    public class When_calling_is_name_available_async_and_user_with_same_name_exists : UserService_specs
    {
        protected static bool Result;

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.ExistsAsync(Username.ToLowerInvariant()))
                .ReturnsAsync(true);
        };

        Because of = async () => Result = await UserService.IsNameAvailableAsync(Username);

        It should_return_false = () => Result.ShouldBeFalse();

        It should_call_exists_async_with_lowercase_name =
            () => UserRepositoryMock.Verify(x => x.ExistsAsync(Username.ToLowerInvariant()), Times.Once);
    }

    [Subject("UserService IsNameAvailableAsync")]
    public class When_calling_is_name_available_async_and_user_with_same_name_does_not_exist : UserService_specs
    {
        protected static bool Result;

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.ExistsAsync(Username.ToLowerInvariant()))
                .ReturnsAsync(false);
        };

        Because of = async () => Result = await UserService.IsNameAvailableAsync(Username);

        It should_return_true = () => Result.ShouldBeTrue();

        It should_call_exists_async_with_lowercase_name =
            () => UserRepositoryMock.Verify(x => x.ExistsAsync(Username.ToLowerInvariant()), Times.Once);
    }

    [Subject("UserService GetAsync")]
    public class When_calling_get_async_and_user_exists : UserService_specs
    {
        protected static Maybe<User> Result;

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(User);
        };

        Because of = async () => Result = await UserService.GetAsync(UserId);

        It should_return_value = () => Result.HasValue.ShouldBeTrue();
    }

    [Subject("UserService GetAsync")]
    public class When_calling_get_async_and_user_does_not_exist : UserService_specs
    {
        protected static Maybe<User> Result;

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(null);
        };

        Because of = async () => Result = await UserService.GetAsync(UserId);

        It should_not_return_value = () => Result.HasValue.ShouldBeFalse();
    }

    [Subject("UserService GetByNameAsync")]
    public class When_calling_get_by_name_async_and_user_exists : UserService_specs
    {
        protected static Maybe<User> Result;

        Establish context = () =>
        {
            Initialize();
            User.SetName(Username);
            UserRepositoryMock
                .Setup(x => x.GetByNameAsync(Username))
                .ReturnsAsync(User);
        };

        Because of = async () => Result = await UserService.GetByNameAsync(Username);

        It should_return_value = () => Result.HasValue.ShouldBeTrue();
    }

    [Subject("UserService GetByNameAsync")]
    public class When_calling_get_by_name_async_and_user_does_not_exist : UserService_specs
    {
        protected static Maybe<User> Result;

        Establish context = () =>
        {
            Initialize();
            User.SetName(Username);
            UserRepositoryMock
                .Setup(x => x.GetByNameAsync(Username))
                .ReturnsAsync(null);
        };

        Because of = async () => Result = await UserService.GetByNameAsync(Username);

        It should_not_return_value = () => Result.HasValue.ShouldBeFalse();
    }

    [Subject("UserService GetByExternalUserIdAsync")]
    public class When_calling_get_by_external_id_async_and_user_exists : UserService_specs
    {
        protected static Maybe<User> Result;

        Establish context = () =>
        {
            Initialize();
            User.SetExternalUserId(ExternalId);
            UserRepositoryMock
                .Setup(x => x.GetByExternalUserIdAsync(ExternalId))
                .ReturnsAsync(User);
        };

        Because of = async () => Result = await UserService.GetByExternalUserIdAsync(ExternalId);

        It should_return_value = () => Result.HasValue.ShouldBeTrue();
    }

    [Subject("UserService GetByExternalUserIdAsync")]
    public class When_calling_get_by_external_id_async_and_user_does_not_exist : UserService_specs
    {
        protected static Maybe<User> Result;

        Establish context = () =>
        {
            Initialize();
            User.SetExternalUserId(ExternalId);
            UserRepositoryMock
                .Setup(x => x.GetByExternalUserIdAsync(ExternalId))
                .ReturnsAsync(null);
        };

        Because of = async () => Result = await UserService.GetByExternalUserIdAsync(ExternalId);

        It should_not_return_value = () => Result.HasValue.ShouldBeFalse();
    }

    [Subject("UserService GetByEmailAsync")]
    public class When_calling_get_by_email_async_and_user_exists : UserService_specs
    {
        protected static Maybe<User> Result;

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByEmailAsync(Email, Provider))
                .ReturnsAsync(User);
        };

        Because of = async () => Result = await UserService.GetByEmailAsync(Email, Provider);

        It should_return_value = () => Result.HasValue.ShouldBeTrue();
    }

    [Subject("UserService GetByExternalUserIdAsync")]
    public class When_calling_get_by_email_async_and_user_does_not_exist : UserService_specs
    {
        protected static Maybe<User> Result;

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByEmailAsync(Email, Provider))
                .ReturnsAsync(null);
        };

        Because of = async () => Result = await UserService.GetByEmailAsync(Email, Provider);

        It should_not_return_value = () => Result.HasValue.ShouldBeFalse();
    }

    [Subject("UserService BrowseAsync")]
    public class When_calling_browse_async_and_users_exist : UserService_specs
    {
        protected static Maybe<PagedResult<User>> Result;

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.BrowseAsync(Query))
                .ReturnsAsync(Users);
        };

        Because of = async () => Result = await UserService.BrowseAsync(Query);

        It should_return_value = () => Result.HasValue.ShouldBeTrue();
        It should_contain_items = () => Result.Value.IsNotEmpty.ShouldBeTrue();
    }

    [Subject("UserService BrowseAsync")]
    public class When_calling_browse_async_and_users_do_not_exist : UserService_specs
    {
        protected static Maybe<PagedResult<User>> Result;

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.BrowseAsync(Query))
                .ReturnsAsync(PagedResult<User>.Empty);
        };

        Because of = async () => Result = await UserService.BrowseAsync(Query);

        It should_return_value = () => Result.HasValue.ShouldBeTrue();
        It should_not_contain_any_items = () => Result.Value.IsEmpty.ShouldBeTrue();
    }

    [Subject("UserService SignUpAsync")]
    public class When_signing_up_async : UserService_specs
    {
        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByEmailAsync(Email, Provider))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByNameAsync(Username))
                .ReturnsAsync(null);
        };

        Because of = async () => await UserService.SignUpAsync(UserId,
                Email, Role, Provider, Password, name: Username);

        It should_call_get_by_user_id_async =
            () => UserRepositoryMock.Verify(x => x.GetByUserIdAsync(UserId), Times.Once);
        It should_call_get_by_email_async =
            () => UserRepositoryMock.Verify(x => x.GetByEmailAsync(Email, Provider), Times.Once);
        It should_call_get_by_name_async =
            () => UserRepositoryMock.Verify(x => x.GetByNameAsync(Username), Times.Once);

        It should_call_add_async = () => UserRepositoryMock.Verify(x => x.AddAsync(Moq.It.Is<User>(u =>
            u.UserId == UserId
            && u.Name == Username.ToLowerInvariant()
            && u.Email == Email
            && u.Role == Role
            && u.Provider == Provider
            && u.Password == Hash)), Times.Once);
    }

    [Subject("UserService SignUpAsync")]
    public class When_signing_up_async_and_user_with_id_already_exists : UserService_specs
    {
        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(User);
            UserRepositoryMock
                .Setup(x => x.GetByEmailAsync(Email, Provider))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByNameAsync(Username))
                .ReturnsAsync(null);
        };

        Because of = () => Exception = Catch.Exception(() =>
        {
            UserService.SignUpAsync(UserId, Email, Role,
                Provider, Password, name: Username).Await();
        });

        It should_call_get_by_user_id_async =
            () => UserRepositoryMock.Verify(x => x.GetByUserIdAsync(UserId), Times.Once);
        It should_not_call_get_by_email_async =
            () => UserRepositoryMock.Verify(x => x.GetByEmailAsync(Email, Provider), Times.Never);
        It should_not_call_get_by_name_async =
            () => UserRepositoryMock.Verify(x => x.GetByNameAsync(Username), Times.Never);
        It should_not_call_add_async = 
            () => UserRepositoryMock.Verify(x => x.AddAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_service_exception =
            () => Exception.ShouldBeOfExactType<ServiceException>();
        It should_throw_exception_with_correct_operation_code = () =>
        {
            var exception = Exception as ServiceException;
            exception.Code.ShouldEqual(OperationCodes.UserIdInUse);
        };
    }

    [Subject("UserService SignUpAsync")]
    public class When_signing_up_async_and_user_with_same_email_and_provider_exists : UserService_specs
    {
        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByEmailAsync(Email, Provider))
                .ReturnsAsync(User);
            UserRepositoryMock
                .Setup(x => x.GetByNameAsync(Username))
                .ReturnsAsync(null);
        };

        Because of = () => Exception = Catch.Exception(() =>
        {
            UserService.SignUpAsync(UserId, Email, Role,
                Provider, Password, name: Username).Await();
        });

        It should_call_get_by_user_id_async =
            () => UserRepositoryMock.Verify(x => x.GetByUserIdAsync(UserId), Times.Once);
        It should_call_get_by_email_async =
            () => UserRepositoryMock.Verify(x => x.GetByEmailAsync(Email, Provider), Times.Once);
        It should_not_call_get_by_name_async =
            () => UserRepositoryMock.Verify(x => x.GetByNameAsync(Username), Times.Never);
        It should_not_call_add_async =
            () => UserRepositoryMock.Verify(x => x.AddAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_service_exception =
            () => Exception.ShouldBeOfExactType<ServiceException>();
        It should_throw_exception_with_correct_operation_code = () =>
        {
            var exception = Exception as ServiceException;
            exception.Code.ShouldEqual(OperationCodes.EmailInUse);
        };
    }

    [Subject("UserService SignUpAsync")]
    public class When_signing_up_async_and_user_with_same_name_exists : UserService_specs
    {
        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByEmailAsync(Email, Provider))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByNameAsync(Username))
                .ReturnsAsync(User);
        };

        Because of = () => Exception = Catch.Exception(() =>
        {
            UserService.SignUpAsync(UserId, Email, Role,
                Provider, Password, name: Username).Await();
        });

        It should_call_get_by_user_id_async =
            () => UserRepositoryMock.Verify(x => x.GetByUserIdAsync(UserId), Times.Once);
        It should_call_get_by_email_async =
            () => UserRepositoryMock.Verify(x => x.GetByEmailAsync(Email, Provider), Times.Once);
        It should_call_get_by_name_async =
            () => UserRepositoryMock.Verify(x => x.GetByNameAsync(Username), Times.Once);
        It should_not_call_add_async =
            () => UserRepositoryMock.Verify(x => x.AddAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_service_exception =
            () => Exception.ShouldBeOfExactType<ServiceException>();
        It should_throw_exception_with_correct_operation_code = () =>
        {
            var exception = Exception as ServiceException;
            exception.Code.ShouldEqual(OperationCodes.NameInUse);
        };
    }


    [Subject("UserService SignUpAsync")]
    public class When_signing_up_async_and_password_is_empty : UserService_specs
    {
        Establish context = () =>
        {
            Initialize();
            Password = "";
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByEmailAsync(Email, Provider))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByNameAsync(Username))
                .ReturnsAsync(null);
        };

        Because of = () => Exception = Catch.Exception(() =>
        {
            UserService.SignUpAsync(UserId, Email, Role,
                Provider, Password, name: Username).Await();
        });

        It should_call_get_by_user_id_async =
            () => UserRepositoryMock.Verify(x => x.GetByUserIdAsync(UserId), Times.Once);
        It should_call_get_by_email_async =
            () => UserRepositoryMock.Verify(x => x.GetByEmailAsync(Email, Provider), Times.Once);
        It should_call_get_by_name_async =
            () => UserRepositoryMock.Verify(x => x.GetByNameAsync(Username), Times.Once);
        It should_not_call_add_async =
            () => UserRepositoryMock.Verify(x => x.AddAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_service_exception =
            () => Exception.ShouldBeOfExactType<ServiceException>();
        It should_throw_exception_with_correct_operation_code = () =>
        {
            var exception = Exception as ServiceException;
            exception.Code.ShouldEqual(OperationCodes.InvalidPassword);
        };
    }

    [Subject("UserService SignUpAsync")]
    public class When_signing_up_async_and_password_is_too_short : UserService_specs
    {
        Establish context = () =>
        {
            Initialize();
            Password = "abc";
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByEmailAsync(Email, Provider))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByNameAsync(Username))
                .ReturnsAsync(null);
        };

        Because of = () => Exception = Catch.Exception(() =>
        {
            UserService.SignUpAsync(UserId, Email, Role,
                Provider, Password, name: Username).Await();
        });

        It should_call_get_by_user_id_async =
            () => UserRepositoryMock.Verify(x => x.GetByUserIdAsync(UserId), Times.Once);
        It should_call_get_by_email_async =
            () => UserRepositoryMock.Verify(x => x.GetByEmailAsync(Email, Provider), Times.Once);
        It should_call_get_by_name_async =
            () => UserRepositoryMock.Verify(x => x.GetByNameAsync(Username), Times.Once);
        It should_not_call_add_async =
            () => UserRepositoryMock.Verify(x => x.AddAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_service_exception =
            () => Exception.ShouldBeOfExactType<DomainException>();
        It should_throw_exception_with_correct_operation_code = () =>
        {
            var exception = Exception as DomainException;
            exception.Code.ShouldEqual(OperationCodes.InvalidPassword);
        };
    }


    [Subject("UserService SignUpAsync")]
    public class When_signing_up_async_and_password_is_too_long : UserService_specs
    {
        Establish context = () =>
        {
            Initialize();
            Password = "abcdefghijabcdefghijabcdefghijabcdefghij" +
                       "abcdefghijabcdefghijabcdefghijabcdefghij" +
                       "abcdefghijabcdefghijabcdefghijabcdefghij";
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByEmailAsync(Email, Provider))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByNameAsync(Username))
                .ReturnsAsync(null);
        };

        Because of = () => Exception = Catch.Exception(() =>
        {
            UserService.SignUpAsync(UserId, Email, Role,
                Provider, Password, name: Username).Await();
        });

        It should_call_get_by_user_id_async =
            () => UserRepositoryMock.Verify(x => x.GetByUserIdAsync(UserId), Times.Once);
        It should_call_get_by_email_async =
            () => UserRepositoryMock.Verify(x => x.GetByEmailAsync(Email, Provider), Times.Once);
        It should_call_get_by_name_async =
            () => UserRepositoryMock.Verify(x => x.GetByNameAsync(Username), Times.Once);
        It should_not_call_add_async =
            () => UserRepositoryMock.Verify(x => x.AddAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_service_exception =
            () => Exception.ShouldBeOfExactType<DomainException>();
        It should_throw_exception_with_correct_operation_code = () =>
        {
            var exception = Exception as DomainException;
            exception.Code.ShouldEqual(OperationCodes.InvalidPassword);
        };
    }

    [Subject("UserService SignUpAsync")]
    public class When_signing_up_async_and_name_is_too_short : UserService_specs
    {
        Establish context = () =>
        {
            Initialize();
            Username = "a";
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByEmailAsync(Email, Provider))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByNameAsync(Username))
                .ReturnsAsync(null);
        };

        Because of = () => Exception = Catch.Exception(() =>
        {
            UserService.SignUpAsync(UserId, Email, Role,
                Provider, Password, name: Username).Await();
        });

        It should_call_get_by_user_id_async =
            () => UserRepositoryMock.Verify(x => x.GetByUserIdAsync(UserId), Times.Once);
        It should_call_get_by_email_async =
            () => UserRepositoryMock.Verify(x => x.GetByEmailAsync(Email, Provider), Times.Once);
        It should_call_get_by_name_async =
            () => UserRepositoryMock.Verify(x => x.GetByNameAsync(Username), Times.Once);
        It should_not_call_add_async =
            () => UserRepositoryMock.Verify(x => x.AddAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_service_exception =
            () => Exception.ShouldBeOfExactType<ArgumentException>();
    }

    [Subject("UserService SignUpAsync")]
    public class When_signing_up_async_and_name_is_too_long : UserService_specs
    {
        Establish context = () =>
        {
            Initialize();
            Username = "abcdefgjijabcdefgjijabcdefgjij" +
                       "abcdefgjijabcdefgjijabcdefgjij";
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByEmailAsync(Email, Provider))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByNameAsync(Username))
                .ReturnsAsync(null);
        };

        Because of = () => Exception = Catch.Exception(() =>
        {
            UserService.SignUpAsync(UserId, Email, Role,
                Provider, Password, name: Username).Await();
        });

        It should_call_get_by_user_id_async =
            () => UserRepositoryMock.Verify(x => x.GetByUserIdAsync(UserId), Times.Once);
        It should_call_get_by_email_async =
            () => UserRepositoryMock.Verify(x => x.GetByEmailAsync(Email, Provider), Times.Once);
        It should_call_get_by_name_async =
            () => UserRepositoryMock.Verify(x => x.GetByNameAsync(Username), Times.Once);
        It should_not_call_add_async =
            () => UserRepositoryMock.Verify(x => x.AddAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_service_exception =
            () => Exception.ShouldBeOfExactType<ArgumentException>();
    }

    [Subject("UserService SignUpAsync")]
    public class When_signing_up_async_and_name_do_not_match_criteria : UserService_specs
    {
        Establish context = () =>
        {
            Initialize();
            Username = "user-asdf ^*(^*&^I sdsasda..";
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByEmailAsync(Email, Provider))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.GetByNameAsync(Username))
                .ReturnsAsync(null);
        };

        Because of = () => Exception = Catch.Exception(() =>
        {
            UserService.SignUpAsync(UserId, Email, Role,
                Provider, Password, name: Username).Await();
        });

        It should_call_get_by_user_id_async =
            () => UserRepositoryMock.Verify(x => x.GetByUserIdAsync(UserId), Times.Once);
        It should_call_get_by_email_async =
            () => UserRepositoryMock.Verify(x => x.GetByEmailAsync(Email, Provider), Times.Once);
        It should_call_get_by_name_async =
            () => UserRepositoryMock.Verify(x => x.GetByNameAsync(Username), Times.Once);
        It should_not_call_add_async =
            () => UserRepositoryMock.Verify(x => x.AddAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_service_exception =
            () => Exception.ShouldBeOfExactType<ArgumentException>();
    }

    [Subject("UserService ChangeNameAsync")]
    public class When_changing_name_async : UserService_specs
    {
        protected static string NewName = "New-username";

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(User);
            UserRepositoryMock
                .Setup(x => x.ExistsAsync(NewName.ToLowerInvariant()))
                .ReturnsAsync(false);
        };

        Because of = () => UserService.ChangeNameAsync(UserId, NewName);

        It should_call_update_async =
            () => UserRepositoryMock.Verify(x => x.UpdateAsync(Moq.It.Is<User>(u =>
                u.UserId == UserId
                && u.Name == NewName.ToLowerInvariant())), Times.Once);
    }
    
    [Subject("UserService ChangeNameAsync")]
    public class When_changing_name_async_and_user_do_not_exist : UserService_specs
    {
        protected static string NewName = "New-username";

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(null);
            UserRepositoryMock
                .Setup(x => x.ExistsAsync(NewName.ToLowerInvariant()))
                .ReturnsAsync(false);
        };

        Because of = () => Exception = Catch.Exception(
            () => UserService.ChangeNameAsync(UserId, NewName).Await());

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


    [Subject("UserService ChangeNameAsync")]
    public class When_changing_name_async_and_new_name_is_in_use : UserService_specs
    {
        protected static string NewName = "New-username";

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(User);
            UserRepositoryMock
                .Setup(x => x.ExistsAsync(NewName.ToLowerInvariant()))
                .ReturnsAsync(true);
        };

        Because of = () => Exception = Catch.Exception(
            () => UserService.ChangeNameAsync(UserId, NewName).Await());

        It should_not_call_update_async =
            () => UserRepositoryMock.Verify(x => x.UpdateAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_service_exception =
            () => Exception.ShouldBeOfExactType<ServiceException>();
        It should_throw_exception_with_name_in_use_code = () =>
        {
            var exception = Exception as ServiceException;
            exception.Code.ShouldEqual(OperationCodes.NameInUse);
        };
    }

    [Subject("UserService ChangeNameAsync")]
    public class When_changing_name_async_and_user_is_already_activated : UserService_specs
    {
        protected static string NewName = "New-username";

        Establish context = () =>
        {
            Initialize();
            User.Activate();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(User);
            UserRepositoryMock
                .Setup(x => x.ExistsAsync(NewName.ToLowerInvariant()))
                .ReturnsAsync(false);
        };

        Because of = () => Exception = Catch.Exception(
            () => UserService.ChangeNameAsync(UserId, NewName).Await());

        It should_not_call_update_async =
            () => UserRepositoryMock.Verify(x => x.UpdateAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_domain_exception =
            () => Exception.ShouldBeOfExactType<DomainException>();
        It should_throw_exception_with_name_already_set_code = () =>
        {
            var exception = Exception as DomainException;
            exception.Code.ShouldEqual(OperationCodes.NameAlreadySet);
        };
    }

    [Subject("UserService ChangeNameAsync")]
    public class When_changing_name_async_and_name_is_empty : UserService_specs
    {
        protected static string NewName = "";

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(User);
            UserRepositoryMock
                .Setup(x => x.ExistsAsync(NewName.ToLowerInvariant()))
                .ReturnsAsync(false);
        };

        Because of = () => Exception = Catch.Exception(
            () => UserService.ChangeNameAsync(UserId, NewName).Await());

        It should_not_call_update_async =
            () => UserRepositoryMock.Verify(x => x.UpdateAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_argument_exception =
            () => Exception.ShouldBeOfExactType<ArgumentException>();
    }

    [Subject("UserService ChangeNameAsync")]
    public class When_changing_name_async_and_name_equals_case_invariant : UserService_specs
    {
        protected static string NewName = Username.ToUpperInvariant();

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(User);
            UserRepositoryMock
                .Setup(x => x.ExistsAsync(NewName.ToLowerInvariant()))
                .ReturnsAsync(false);
        };

        Because of = () => UserService.ChangeNameAsync(UserId, NewName).Await();

        It should_call_update_async =
            () => UserRepositoryMock.Verify(x => x.UpdateAsync(Moq.It.Is<User>(u =>
                u.UserId == UserId
                && u.Name == NewName.ToLowerInvariant())), Times.Once);
    }

    [Subject("UserService ChangeNameAsync")]
    public class When_changing_name_async_and_name_is_too_short : UserService_specs
    {
        protected static string NewName = "A";

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(User);
            UserRepositoryMock
                .Setup(x => x.ExistsAsync(NewName.ToLowerInvariant()))
                .ReturnsAsync(false);
        };

        Because of = () => Exception = Catch.Exception(
            () => UserService.ChangeNameAsync(UserId, NewName).Await());

        It should_not_call_update_async =
            () => UserRepositoryMock.Verify(x => x.UpdateAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_argument_exception =
            () => Exception.ShouldBeOfExactType<ArgumentException>();
    }

    [Subject("UserService ChangeNameAsync")]
    public class When_changing_name_async_and_name_is_too_long : UserService_specs
    {
        protected static string NewName = "AbcdefghijAbcdefghijAbcdefghij" +
                                          "AbcdefghijAbcdefghijAbcdefghij";

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(User);
            UserRepositoryMock
                .Setup(x => x.ExistsAsync(NewName.ToLowerInvariant()))
                .ReturnsAsync(false);
        };

        Because of = () => Exception = Catch.Exception(
            () => UserService.ChangeNameAsync(UserId, NewName).Await());

        It should_not_call_update_async =
            () => UserRepositoryMock.Verify(x => x.UpdateAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_argument_exception =
            () => Exception.ShouldBeOfExactType<ArgumentException>();
    }

    [Subject("UserService ChangeNameAsync")]
    public class When_changing_name_async_and_name_do_not_match_the_criteria : UserService_specs
    {
        protected static string NewName = "Ab cd ef .. *&*(";

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(User);
            UserRepositoryMock
                .Setup(x => x.ExistsAsync(NewName.ToLowerInvariant()))
                .ReturnsAsync(false);
        };

        Because of = () => Exception = Catch.Exception(
            () => UserService.ChangeNameAsync(UserId, NewName).Await());

        It should_not_call_update_async =
            () => UserRepositoryMock.Verify(x => x.UpdateAsync(Moq.It.IsAny<User>()), Times.Never);
        It should_throw_argument_exception =
            () => Exception.ShouldBeOfExactType<ArgumentException>();
    }

    [Subject("UserService ChangeAvatarAsync")]
    public class When_changing_avatar_async : UserService_specs
    {
        protected static string NewUrl = "url";

        Establish context = () =>
        {
            Initialize();
            UserRepositoryMock
                .Setup(x => x.GetByUserIdAsync(UserId))
                .ReturnsAsync(User);
        };

        Because of = () => UserService.ChangeAvatarAsync(UserId, NewUrl);

        It should_call_update_async =
            () => UserRepositoryMock.Verify(x => x.UpdateAsync(Moq.It.Is<User>(u =>
                u.UserId == UserId
                && u.PictureUrl == NewUrl)), Times.Once);
    }

    [Subject("UserService ChangeAvatarAsync")]
    public class When_changing_avatar_async_and_user_do_not_exist : UserService_specs
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
            () => UserService.ChangeAvatarAsync(UserId, NewUrl).Await());

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