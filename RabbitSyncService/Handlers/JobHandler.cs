using System;
using System.Threading.Tasks;
using Apache.NMS;
using Apache.NMS.Util;
using Rabbit.Bridge.Messages;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Retry.Simple;

namespace RabbitSyncService.Handlers
{
    public class JobHandler : IHandleMessages<Job>, IHandleMessages<IFailed<Job>>
    {
        private object lock1 = new Object();

        // Для теста http://localhost:5000/api/test/test-rabbit-publish
        public async Task Handle(Job message)
        {
            Console.WriteLine("Job: " + message.JobNumber);

/*
            Console.WriteLine(message.JobNumber);
            lock (lock1)
            {
                SendToAMQ();
                throw new NotImplementedException();
            }
*/
        }

        public async Task Handle(IFailed<Job> failedMessage)
        {
            await bus.Advanced.TransportMessage.Defer(TimeSpan.FromSeconds(30));
        }

        private void SendToAMQ()
        {
            Uri connecturi = new Uri("activemq:tcp://192.168.52.87:61616?wireFormat.maxInactivityDuration=5000000");

            Console.WriteLine("About to connect to " + connecturi);

            // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.
            IConnectionFactory factory = new NMSConnectionFactory(connecturi);

            using (IConnection connection = factory.CreateConnection("smx", "smx"))
            using (ISession session = connection.CreateSession())
            {
                // Examples for getting a destination:
                //
                // Hard coded destinations:
                //    IDestination destination = session.GetQueue("FOO.BAR");
                //    Debug.Assert(destination is IQueue);
                //    IDestination destination = session.GetTopic("FOO.BAR");
                //    Debug.Assert(destination is ITopic);
                //
                // Embedded destination type in the name:
                //    IDestination destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");
                //    Debug.Assert(destination is IQueue);
                //    IDestination destination = SessionUtil.GetDestination(session, "topic://FOO.BAR");
                //    Debug.Assert(destination is ITopic);
                //
                // Defaults to queue if type is not specified:
                //    IDestination destination = SessionUtil.GetDestination(session, "FOO.BAR");
                //    Debug.Assert(destination is IQueue);
                //
                // .NET 3.5 Supports Extension methods for a simplified syntax:
                //    IDestination destination = session.GetDestination("queue://FOO.BAR");
                //    Debug.Assert(destination is IQueue);
                //    IDestination destination = session.GetDestination("topic://FOO.BAR");
                //    Debug.Assert(destination is ITopic);

                IDestination destination = SessionUtil.GetDestination(session, "queue://headwks.docsvision.in");
                Console.WriteLine("Using destination: " + destination);

                // Create a consumer and producer
                using (IMessageConsumer consumer = session.CreateConsumer(destination))
                using (IMessageProducer producer = session.CreateProducer(destination))
                {
                    // Start the connection so that messages will be processed.
                    connection.Start();
                    producer.DeliveryMode = MsgDeliveryMode.Persistent;

                    // Send a message
                    ITextMessage request = session.CreateTextMessage("Hello World!");
                    request.NMSCorrelationID = "abc";
                    request.Properties["NMSXGroupID"] = "cheese";
                    request.Properties["myHeader"] = "Cheddar";

                    producer.Send(request);

                    // Consume a message
                    ITextMessage message = consumer.Receive() as ITextMessage;
                    if (message == null)
                    {
                        Console.WriteLine("No message received!");
                    }
                    else
                    {
                        Console.WriteLine("Received message with ID:   " + message.NMSMessageId);
                        Console.WriteLine("Received message with text: " + message.Text);
                    }
                }
            }
        }

        readonly IBus bus;


        public JobHandler(IBus bus)
        {
            this.bus = bus;
        }
    }
}
