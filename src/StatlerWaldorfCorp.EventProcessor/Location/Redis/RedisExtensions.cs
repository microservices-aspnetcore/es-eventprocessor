using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace StatlerWaldorfCorp.EventProcessor.Location.Redis
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedisConnectionMultiplexer(this IServiceCollection services,
            IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var redisConfig = config.GetSection("redis:configstring").Value;
            services.AddSingleton(typeof(IConnectionMultiplexer), ConnectionMultiplexer.Connect(redisConfig));
            return services;
        }
    }
}