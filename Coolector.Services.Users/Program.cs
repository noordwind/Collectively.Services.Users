using Coolector.Common.Commands.Users;
using Coolector.Common.Host;
using Coolector.Services.Users.Framework;

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
                .Build()
                .Run();
        }
    }
}
