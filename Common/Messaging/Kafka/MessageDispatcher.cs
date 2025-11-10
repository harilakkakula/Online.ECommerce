using Common.Messaging.Kafka.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Common.Messaging.Kafka
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRetryPolicy _retryPolicy;
        private readonly IDeadLetterProducer _deadLetterProducer;

        public MessageDispatcher(
            IServiceScopeFactory scopeFactory,
            IRetryPolicy retryPolicy,
            IDeadLetterProducer deadLetterProducer)
        {
            _scopeFactory = scopeFactory;
            _retryPolicy = retryPolicy;
            _deadLetterProducer = deadLetterProducer;
        }

        public void Dispatch(string topic, string message)
        {
            using var scope = _scopeFactory.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IEventHandler>();

            var handler = handlers.FirstOrDefault(h => h.Topic == topic);
            if (handler == null)
            {
                Console.WriteLine($" No handler registered for topic '{topic}'.");
                return;
            }

            _retryPolicy.Execute(
                () =>
                {
                    Console.WriteLine($" Dispatching message from '{topic}' to {handler.GetType().Name}");
                    handler.Handle(message);
                },
                onFailure: ex =>
                {
                    Console.WriteLine($" Failed processing '{topic}' after retries: {ex.Message}");
                    _deadLetterProducer.Send(message, ex.Message);
                });
        }
    }
}
