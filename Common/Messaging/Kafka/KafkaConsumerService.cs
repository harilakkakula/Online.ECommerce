using Common.Messaging.Kafka.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Messaging.Kafka
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _bootstrapServers;

        public KafkaConsumerService(
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _bootstrapServers = configuration["KAFKA__BOOTSTRAPSERVERS"] ?? "kafka:9092";
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() => Consume(stoppingToken), stoppingToken);
            return Task.CompletedTask;
        }

        private void Consume(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IEventHandler>().ToList();

            if (!handlers.Any())
            {
                Console.WriteLine(" No Kafka event handlers registered.");
                return;
            }

            var topics = handlers.Select(h => h.Topic).Distinct().ToList();

            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "service-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(topics);

            Console.WriteLine($" Kafka consumer started. Listening to: {string.Join(", ", topics)}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(stoppingToken);

                    // Process each message in its own scoped lifetime
                    using var messageScope = _serviceProvider.CreateScope();
                    var dispatcher = messageScope.ServiceProvider.GetRequiredService<IMessageDispatcher>();
                    dispatcher.Dispatch(cr.Topic, cr.Message.Value);
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($" Kafka error: {ex.Error.Reason}");
                }
            }
        }
    }
}
