using Coolector.Common.Host;
using Coolector.Services.Users.Framework;
using Coolector.Services.Users.Shared.Commands;
using Coolector.Services.Users.Shared.Commands.Facebook;

namespace Coolector.Services.Users
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
                .SubscribeToCommand<ChangeUserName>()
                .SubscribeToCommand<ChangeAvatar>()
                .SubscribeToCommand<ChangePassword>()
                .SubscribeToCommand<ResetPassword>()
                .SubscribeToCommand<SetNewPassword>()
                .SubscribeToCommand<PostMessageOnFacebookWall>()
                .Build()
                .Run();
        }
    }
}
