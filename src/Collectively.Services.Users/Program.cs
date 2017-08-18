using Collectively.Common.Host;
using Collectively.Services.Users.Framework;
using Collectively.Messages.Commands.Users;

namespace Collectively.Services.Users
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebServiceHost
                .Create<Startup>(args: args)
                .UseAutofac(Bootstrapper.LifetimeScope)
                .UseRabbitMq(queueName: typeof(Program).Namespace)
                .SubscribeToCommand<SignUp>()
                .SubscribeToCommand<SignOut>()
                .SubscribeToCommand<ActivateAccount>()
                .SubscribeToCommand<ChangeUsername>()
                .SubscribeToCommand<UploadAvatar>()
                .SubscribeToCommand<RemoveAvatar>()
                .SubscribeToCommand<ChangePassword>()
                .SubscribeToCommand<ResetPassword>()
                .SubscribeToCommand<SetNewPassword>()
                .SubscribeToCommand<PostOnFacebookWall>()
                .SubscribeToCommand<DeleteAccount>()
                .SubscribeToCommand<LockAccount>()
                .SubscribeToCommand<UnlockAccount>()
                .Build()
                .Run();
        }
    }
}
