using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StatlerWaldorfCorp.EventProcessor.Events;
using StatlerWaldorfCorp.EventProcessor.Location;
using StatlerWaldorfCorp.EventProcessor.Location.Redis;
using StatlerWaldorfCorp.EventProcessor.Queues;
using StatlerWaldorfCorp.EventProcessor.Queues.AMQP;
using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.CloudFoundry.Connector.Redis;

namespace StatlerWaldorfCorp.EventProcessor
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory) 
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            
            var builder = new ConfigurationBuilder()                
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
		        .AddEnvironmentVariables()		    
                .AddCloudFoundry();

	        Configuration = builder.Build();    		        
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services) 
        {
            services.AddMvc();
            services.AddOptions();

            services.Configure<CloudFoundryApplicationOptions>(Configuration);
            services.Configure<CloudFoundryServicesOptions>(Configuration);            

            services.AddDistributedRedisCache(Configuration);
            services.AddRedisConnectionMultiplexer(Configuration);

            services.Configure<QueueOptions>(Configuration.GetSection("QueueOptions"));
            services.AddTransient(typeof(IConnectionFactory), typeof(AMQPConnectionFactory));
            services.AddTransient(typeof(EventingBasicConsumer), typeof(AMQPEventingConsumer));
            services.AddTransient(typeof(ILocationCache), typeof(RedisLocationCache));

            services.AddSingleton(typeof(IEventSubscriber), typeof(AMQPEventSubscriber));            
            services.AddSingleton(typeof(IEventEmitter), typeof(AMQPEventEmitter));            
            services.AddSingleton(typeof(IEventProcessor), typeof(MemberLocationEventProcessor));
        }

        public void Configure(IApplicationBuilder app, 
                IHostingEnvironment env, 
                ILoggerFactory loggerFactory,
                IEventSubscriber subscriber,
                IEventEmitter eventEmitter) 
        {                                   
            app.UseMvc();

            subscriber.Subscribe();
        }
    }
}