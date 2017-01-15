using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Linq;
using Steeltoe.Extensions.Configuration.CloudFoundry;

namespace StatlerWaldorfCorp.EventProcessor.Queues.AMQP
{
    public abstract class AMQPClientBase 
    {
        protected Service rabbitServiceBinding;

        protected ConnectionFactory connectionFactory;

        protected QueueOptions queueOptions; 

        public AMQPClientBase(IOptions<CloudFoundryServicesOptions> cfOptions,
            IOptions<QueueOptions> queueOptions)
        {
            this.rabbitServiceBinding = cfOptions.Value.Services.FirstOrDefault( s => s.Name == "rabbitmq");
            this.queueOptions = queueOptions.Value;

            connectionFactory = InitializeConnectionFactory();
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