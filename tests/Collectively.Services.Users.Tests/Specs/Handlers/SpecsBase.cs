using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using RawRabbit;
using RawRabbit.Pipe;

namespace Collectively.Services.Users.Tests.Specs.Handlers
{
    public abstract class SpecsBase
    {
        protected static Mock<IBusClient> BusClientMock;

        protected static void InitializeBus()
        {
            BusClientMock = new Mock<IBusClient>();
        }

        //Temporary fix due to extension method PublishAsync.
        protected static void VerifyPublishAsync<TMessage>(TMessage message, Func<Times> times)
        => BusClientMock.Verify(x => x.InvokeAsync(Moq.It.IsAny<Action<IPipeBuilder>>(),
                Moq.It.IsAny<Action<IPipeContext>>(), Moq.It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }
}