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
                .Create<Startup>(port: 10001)
                .UseAutofac(Bootstrapper.LifetimeScope)
                .UseRabbitMq(queueName: typeof(Program).Namespace)
                .SubscribeToCommand<SignIn>()
                .SubscribeToCommand<SignUp>()
                .SubscribeToCommand<SignOut>()
                .SubscribeToCommand<ChangeUsername>()
                .SubscribeToCommand<UploadAvatar>()
                .SubscribeToCommand<RemoveAvatar>()
                .SubscribeToCommand<ChangePassword>()
                .SubscribeToCommand<ResetPassword>()
                .SubscribeToCommand<SetNewPassword>()
                .SubscribeToCommand<PostOnFacebookWall>()
                .Build()
                .Run();
        }
    }
}
