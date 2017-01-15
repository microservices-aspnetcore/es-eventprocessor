using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace StatlerWaldorfCorp.EventProcessor.Queues.AMQP
{
    public class AMQPConnectionFactory : ConnectionFactory
    {
        protected Service rabbitServiceBinding;

        public AMQPConnectionFactory(
            ILogger<AMQPConnectionFactory> logger,
            IOptions<CloudFoundryServicesOptions> cfOptions) : base()
        {
            this.rabbitServiceBinding = cfOptions.Value.Services.FirstOrDefault( s => s.Name == "rabbitmq");

            this.UserName = rabbitServiceBinding.Credentials["username"].Value;
            this.Password = rabbitServiceBinding.Credentials["password"].Value;
            this.VirtualHost = rabbitServiceBinding.Credentials["vhost"].Value;
            this.HostName = rabbitServiceBinding.Credentials["hostname"].Value;
            this.Uri = rabbitServiceBinding.Credentials["uri"].Value;

            logger.LogInformation($"AMQP Connection configured for URI : {rabbitServiceBinding.Credentials["uri"].Value}");
        }
    }
}