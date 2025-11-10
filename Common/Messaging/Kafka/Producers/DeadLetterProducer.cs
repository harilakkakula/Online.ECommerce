using Common.Messaging.Kafka.Interfaces;
using Confluent.Kafka;
using System;
using System.Text.Json;

namespace Common.Messaging.Kafka.Producers
{
    public class DeadLetterProducer : IDeadLetterProducer
    {
        private readonly string _bootstrapServers;

        public DeadLetterProducer(string bootstrapServers)
        {
            _bootstrapServers = bootstrapServers;
        }

        public void Send(string message, string error)
        {
            var config = new ProducerConfig { BootstrapServers = _bootstrapServers };
            using var producer = new ProducerBuilder<Null, string>(config).Build();

            var payload = JsonSerializer.Serialize(new
            {
                OriginalMessage = message,
                Error = error,
                FailedAt = DateTime.UtcNow
            });

            producer.Produce("dead-letter", new Message<Null, string> { Value = payload });
            Console.WriteLine("📤 Sent failed message to 'dead-letter' topic.");
        }
    }
}
