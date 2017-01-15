using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StatlerWaldorfCorp.EventProcessor.Events;
using Steeltoe.Extensions.Configuration.CloudFoundry;

namespace StatlerWaldorfCorp.EventProcessor.Queues.AMQP
{
    public class AMQPEventSubscriber : IEventSubscriber
    {
        private ILogger logger;
        
        private IConnection connection;

        private IConnectionFactory connectionFactory;
        private EventingBasicConsumer consumer;
        private QueueOptions queueOptions;
        private string consumerTag;
        private IModel channel;

        public AMQPEventSubscriber(ILogger<AMQPEventSubscriber> logger,
            IOptions<CloudFoundryServicesOptions> cfOptions,
            IOptions<QueueOptions> queueOptions,
            IConnectionFactory connectionFactory) 
        {
            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.queueOptions = queueOptions.Value;

            Initialize();             
        }

        private void Initialize()
        {
            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: queueOptions.MemberLocationRecordedEventQueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            consumer = new EventingBasicConsumer(channel);

            consumer.Received += (ch, ea) => {
                var body = ea.Body;
                var msg = Encoding.UTF8.GetString(body);
                var evt = JsonConvert.DeserializeObject<MemberLocationRecordedEvent>(msg);
                logger.LogInformation($"Received incoming event, {body.Length} bytes.");
                if (MemberLocationRecordedEventReceived != null) {
                    MemberLocationRecordedEventReceived(evt);
                }
                ((IModel)ch).BasicAck(ea.DeliveryTag, false);
            };
        }
        
        public event MemberLocationRecordedEventReceivedDelegate MemberLocationRecordedEventReceived;

        public void Subscribe()
        {
            consumerTag = channel.BasicConsume(queueOptions.MemberLocationRecordedEventQueueName, false, consumer);       
        }

        public void Unsubscribe()
        {
            channel.BasicCancel(consumerTag);
        }
    }
}