using System;
using System.Threading.Tasks;
using Rabbit.Bridge.Messages;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Retry.Simple;

namespace RabbitSyncService.Handlers
{
    public class RetryHandler : IHandleMessages<RetryExample>, IHandleMessages<IFailed<RetryExample>>
    {
        private object lock1 = new Object();

        // Для теста http://localhost:5000/api/test/test-rabbit-publish-retry
        public async Task Handle(RetryExample message)
        {
            Console.WriteLine($"Retry: {DateTime.Now.ToShortTimeString()}, origin date: {message.CreatedAt.ToShortTimeString()}" );
            if(message.CreatedAt.AddMinutes(10) > DateTime.Now)
                await bus.Advanced.TransportMessage.Defer(TimeSpan.FromSeconds(30));
        }

        public async Task Handle(IFailed<RetryExample> failedMessage)
        {
            await bus.Advanced.TransportMessage.Defer(TimeSpan.FromSeconds(30));
        }

        readonly IBus bus;


        public RetryHandler(IBus bus)
        {
            this.bus = bus;
        }
    }
}
