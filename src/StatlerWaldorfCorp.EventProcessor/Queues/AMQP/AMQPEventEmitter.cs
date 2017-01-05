using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StatlerWaldorfCorp.EventProcessor.Events;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System.Linq;
using RabbitMQ.Client;
using System.Text;

namespace StatlerWaldorfCorp.EventProcessor.Queues.AMQP
{
    public class AMQPEventEmitter : IEventEmitter
    {
        private ILogger<AMQPEventEmitter> logger;
        private Service rabbitServiceBinding;

        private ConnectionFactory connectionFactory;

        private QueueOptions queueOptions;
        
        public AMQPEventEmitter(ILogger<AMQPEventEmitter> logger,
            IOptions<CloudFoundryServicesOptions> cfOptions,
            IOptions<QueueOptions> queueOptions)
        {         
            this.logger = logger;            
            this.rabbitServiceBinding = cfOptions.Value.Services.FirstOrDefault( s => s.Name == "rabbitmq");
            this.queueOptions = queueOptions.Value;

            connectionFactory = InitializeConnectionFactory();

            logger.LogInformation($"Emitting events on queue {this.queueOptions.ProximityDetectedEventQueueName}");
            logger.LogInformation($"AMQP Connection configured for URI : {rabbitServiceBinding.Credentials["uri"].Value}");
        }

        public void EmitProximityDetectedEvent(ProximityDetectedEvent proximityDetectedEvent)
        {
             using (IConnection conn = connectionFactory.CreateConnection()) {
                using (IModel channel = conn.CreateModel()) {
                    channel.QueueDeclare(
                        queue: queueOptions.ProximityDetectedEventQueueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );
                    string jsonPayload = proximityDetectedEvent.toJson();
                    var body = Encoding.UTF8.GetBytes(jsonPayload);
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: queueOptions.ProximityDetectedEventQueueName,
                        basicProperties: null,
                        body: body
                    );
                    logger.LogInformation($"Emitted proximity event of {jsonPayload.Length} bytes to queue.");
                }
            }
        }

        private ConnectionFactory InitializeConnectionFactory()
        {
            var factory = new ConnectionFactory();
            
            factory.UserName = rabbitServiceBinding.Credentials["username"].Value;
            factory.Password = rabbitServiceBinding.Credentials["password"].Value;
            factory.VirtualHost = rabbitServiceBinding.Credentials["vhost"].Value;
            factory.HostName = rabbitServiceBinding.Credentials["hostname"].Value;
            factory.Uri = rabbitServiceBinding.Credentials["uri"].Value;

            return factory;
        }
    }
}