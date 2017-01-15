using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StatlerWaldorfCorp.EventProcessor.Queues.AMQP;
using StatlerWaldorfCorp.EventProcessor.Queues;
using RabbitMQ.Client;
using Microsoft.Extensions.Options;
using StatlerWaldorfCorp.EventProcessor.Events;
using System;

namespace StatlerWaldorfCorp.EventProcessor.Tests.Queues.AMQP
{
    public class AMQPEventEmitterTest
    {
        [Fact]
        public void AmqpEmitterUsesBasicPublish()
        {
            var logger = new Mock<ILogger<AMQPEventEmitter>>();    
            var queueOptions = new QueueOptions {
                ProximityDetectedEventQueueName = "proximitydetected",
                MemberLocationRecordedEventQueueName = "memberlocationrecorded"
            };            
            var queueOptionsWrapper = new Mock<IOptions<QueueOptions>>();
            queueOptionsWrapper.Setup( o => o.Value ).Returns(queueOptions);

            var factory = new Mock<IConnectionFactory>();
            var connection = new Mock<IConnection>();
            var model = new Mock<IModel>();

            model.Setup( m => m.BasicPublish(It.IsAny<string>(), 
                        It.Is<string>( rk => rk == queueOptions.ProximityDetectedEventQueueName),
                        It.IsAny<bool>(), 
                        It.IsAny<IBasicProperties>(), 
                        It.IsAny<byte[]>()) );
            connection.Setup( c => c.CreateModel()).Returns(model.Object);
            factory.Setup( f=> f.CreateConnection()).Returns(connection.Object);

            var emitter = new AMQPEventEmitter(logger.Object, queueOptionsWrapper.Object, factory.Object);

            var evt = new ProximityDetectedEvent {
                SourceMemberID = Guid.NewGuid(),
                TargetMemberID = Guid.NewGuid()
            };            
            emitter.EmitProximityDetectedEvent(evt);

            model.VerifyAll();
        }
    }
}