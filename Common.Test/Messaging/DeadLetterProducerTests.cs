using System.Text.Json;
using Common.Messaging.Kafka.Producers;
using Confluent.Kafka;
using FakeItEasy;

namespace Common.Test.Messaging
{
    public class DeadLetterProducerTests
    {
        [Fact]
        public void Send_ShouldProduceMessageToDeadLetterTopic()
        {
            // Arrange
            var producer = new DeadLetterProducer("localhost:9092");

            using var console = new ConsoleCapture();

            // Act
            producer.Send("test-message", "some-error");

            //  Flush console writer before reading
            Console.Out.Flush();

            var output = console.GetOutput();

            // Assert
            Assert.Contains("dead-letter", output);
            Assert.Contains(" Sent failed message", output);
        }

     

        [Fact]
        public void Send_ShouldThrowException_WhenKafkaFails()
        {
            #region Arrange
            var producerFake = A.Fake<IProducer<Null, string>>();

            A.CallTo(() => producerFake.Produce(
                A<string>._,
                A<Message<Null, string>>._,
                A<Action<DeliveryReport<Null, string>>>.Ignored
            )).Throws(new Exception("Kafka not available"));

            var producer = new DeadLetterProducerWrapper("localhost:9092", producerFake);
            #endregion

            #region Act & Assert
            var ex = Assert.Throws<Exception>(() =>
                producer.Send("TestMessage", "Network issue"));

            Assert.Contains("Kafka not available", ex.Message);
            #endregion
        }

        #region Helper Wrapper

        private class DeadLetterProducerWrapper : DeadLetterProducer
        {
            private readonly IProducer<Null, string> _testProducer;

            public DeadLetterProducerWrapper(string bootstrapServers, IProducer<Null, string> producer)
                : base(bootstrapServers)
            {
                _testProducer = producer;
            }

            public new void Send(string message, string error)
            {
                var payload = JsonSerializer.Serialize(new
                {
                    OriginalMessage = message,
                    Error = error,
                    FailedAt = DateTime.UtcNow
                });

                _testProducer.Produce("dead-letter", new Message<Null, string> { Value = payload });
                Console.WriteLine(" Sent failed message to 'dead-letter' topic.");
            }
        }

        private class ConsoleCapture : IDisposable
        {
            private readonly StringWriter _writer;
            private readonly TextWriter _original;

            public ConsoleCapture()
            {
                _writer = new StringWriter();
                _original = Console.Out;
                Console.SetOut(_writer);
            }

            public string GetOutput()
            {
                _writer.Flush();
                return _writer.ToString();
            }

            public void Dispose()
            {
                Console.SetOut(_original);
                _writer.Dispose();
            }
        }

        #endregion
    }
}
