using Confluent.Kafka;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Integration
{
    public interface IProducerService
    {
        Task ProduceAsync(string topic, object message);
    }

    public class ProducerService : IProducerService
    {
        private readonly IProducer<Null, string> _producer;
        private readonly AsyncPolicy _resiliencePolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public ProducerService(string bootstrapServers)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                MessageTimeoutMs = 5000
            };

            // Try building the producer until Kafka is reachable
            var builder = new ProducerBuilder<Null, string>(config);
            while (true)
            {
                try
                {
                    _producer = builder.Build();
                    Console.WriteLine("Connected to Kafka successfully.");
                    break;
                }
                catch
                {
                    Console.WriteLine("Waiting for Kafka...");
                    Thread.Sleep(2000);
                }
            }

            // Retry Policy — retry 3 times with exponential backoff
            var retryPolicy = Policy
                .Handle<KafkaException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, ts, ctx) =>
                    {
                        Console.WriteLine($"Retrying Kafka send: {ex.Message}");
                    });

            // Circuit Breaker Policy — open after 3 failures for 30 seconds
            _circuitBreakerPolicy = Policy
                .Handle<KafkaException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (ex, ts) =>
                    {
                        Console.WriteLine($" Kafka circuit opened for {ts.TotalSeconds}s: {ex.Message}");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine(" Kafka circuit closed again.");
                    },
                    onHalfOpen: () =>
                    {
                        Console.WriteLine(" Kafka circuit half-open, testing connection...");
                    });

            _resiliencePolicy = Policy.WrapAsync(retryPolicy, _circuitBreakerPolicy);
        }

        public async Task ProduceAsync(string topic, object message)
        {
            var json = JsonSerializer.Serialize(message);

            await _resiliencePolicy.ExecuteAsync(async () =>
            {
                await _producer.ProduceAsync(topic, new Message<Null, string> { Value = json });
                Console.WriteLine($"Produced message to {topic}: {json}");
            });
        }
    }
}
