using Common.Messaging.Kafka.Interfaces;
using Common.Messaging.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Common.Messaging.Kafka.Policies;
using Common.Messaging.Kafka.Producers;

namespace Common.Messaging.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaMessaging(this IServiceCollection services, string bootstrapServers)
        {
            services.AddSingleton<IRetryPolicy>(_ => new ExponentialRetryPolicy(3, 500));
            services.AddSingleton<IDeadLetterProducer>(_ => new DeadLetterProducer(bootstrapServers));

            // Dispatcher must be Scoped since handlers use scoped services (like DbContext)
            services.AddScoped<IMessageDispatcher, MessageDispatcher>();

            return services;
        }
    }
}
