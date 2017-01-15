using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StatlerWaldorfCorp.EventProcessor.Queues.AMQP;
using Microsoft.Extensions.Options;
using StatlerWaldorfCorp.EventProcessor.Queues;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System;
using StatlerWaldorfCorp.EventProcessor.Events;
using Newtonsoft.Json;

namespace StatlerWaldorfCorp.EventProcessor.Tests.Queues.AMQP
{
    public class AMQPEventSubscriberTest
    {
        [Fact]
        public void EventSubscriberListensOnAppropriateQueue_And_InvokesMemberLocationReceivedEvent()
        {
            var logger = new Mock<ILogger<AMQPEventingConsumer>>();
            var logger2 = new Mock<ILogger<AMQPEventSubscriber>>();
             var queueOptions = new QueueOptions {
                ProximityDetectedEventQueueName = "proximitydetected",
                MemberLocationRecordedEventQueueName = "memberlocationrecorded"
            };   
            var options = new Mock<IOptions<QueueOptions>>();
            options.Setup( o => o.Value).Returns(queueOptions);

            var factory = new Mock<IConnectionFactory>();
            var connection = new Mock<IConnection>();
            var model = new Mock<IModel>();
            connection.Setup( c => c.CreateModel()).Returns(model.Object);
            factory.Setup( f=> f.CreateConnection()).Returns(connection.Object);

            var consumer = new FakeConsumer(logger.Object, factory.Object);

            AMQPEventSubscriber subscriber = new AMQPEventSubscriber(logger2.Object, options.Object, consumer);
            MemberLocationRecordedEvent deliveredMLRE = null;
            subscriber.MemberLocationRecordedEventReceived += (mlre) => {                
                deliveredMLRE = mlre;
            };

            consumer.TriggerBasicDeliver();
            Assert.NotNull(deliveredMLRE);
            Assert.Equal(consumer.MLRE.MemberID, deliveredMLRE.MemberID);
            Assert.Equal(consumer.MLRE.Longitude, deliveredMLRE.Longitude);            
        }
    }

    class FakeConsumer : AMQPEventingConsumer
    {
        private MemberLocationRecordedEvent mlre;

        public FakeConsumer(
            ILogger<AMQPEventingConsumer> logger,
            IConnectionFactory factory) : base(logger, factory) 
        {
            mlre = new MemberLocationRecordedEvent {
                Origin = "test",
                Latitude = 1.0,
                Longitude = 1.0,
                MemberID = Guid.NewGuid(),
                RecordedTime = DateTime.UtcNow.Ticks,
                ReportID = Guid.NewGuid()
            };
        }

        public MemberLocationRecordedEvent MLRE { get { return mlre; }}

        public void TriggerBasicDeliver() {
            BasicDeliverEventArgs ea = new BasicDeliverEventArgs();
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mlre));
            HandleBasicDeliver("ct", 0, false, "exchange", "routing", null, body);
        }
    }
}