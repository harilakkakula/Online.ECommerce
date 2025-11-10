using Common.Messaging.Kafka;
using Common.Messaging.Kafka.Interfaces;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Tests.Messaging.Kafka
{
    public class MessageDispatcherTests
    {
        private readonly IServiceScopeFactory _scopeFactoryFake;
        private readonly IServiceScope _scopeFake;
        private readonly IServiceProvider _providerFake;
        private readonly IRetryPolicy _retryPolicyFake;
        private readonly IDeadLetterProducer _deadLetterProducerFake;

        private readonly MessageDispatcher _dispatcher;

        public MessageDispatcherTests()
        {
            _scopeFactoryFake = A.Fake<IServiceScopeFactory>();
            _scopeFake = A.Fake<IServiceScope>();
            _providerFake = A.Fake<IServiceProvider>();
            _retryPolicyFake = A.Fake<IRetryPolicy>();
            _deadLetterProducerFake = A.Fake<IDeadLetterProducer>();

            A.CallTo(() => _scopeFactoryFake.CreateScope()).Returns(_scopeFake);
            A.CallTo(() => _scopeFake.ServiceProvider).Returns(_providerFake);

            _dispatcher = new MessageDispatcher(_scopeFactoryFake, _retryPolicyFake, _deadLetterProducerFake);
        }

        #region Positive Case

        [Fact]
        public void Dispatch_ShouldCallHandler_WhenHandlerExists()
        {
            // Arrange
            var handlerFake = A.Fake<IEventHandler>();
            A.CallTo(() => handlerFake.Topic).Returns("user.created");

            var handlers = new List<IEventHandler> { handlerFake };

            //  Fake the non-extension GetService() call
            A.CallTo(() => _providerFake.GetService(typeof(IEnumerable<IEventHandler>)))
                .Returns(handlers);

            var message = "{ \"Id\": 1 }";

            // Act
            _dispatcher.Dispatch("user.created", message);

            // Assert
            A.CallTo(() => _retryPolicyFake.Execute(A<Action>._, A<Action<Exception>>._))
                .MustHaveHappenedOnceExactly();
        }

        #endregion

        #region Negative Case: No Handler Found

        [Fact]
        public void Dispatch_ShouldLogWarning_WhenNoHandlerRegisteredForTopic()
        {
            // FIXED: Fake GetService instead of GetServices<T>()
            A.CallTo(() => _providerFake.GetService(typeof(IEnumerable<IEventHandler>)))
                .Returns(new List<IEventHandler>());

            using var console = new ConsoleCapture();

            // Act
            _dispatcher.Dispatch("order.unknown", "{ \"Test\": true }");

            // Assert
            var output = console.GetOutput();
            Assert.Contains("No handler registered", output);

            A.CallTo(() => _retryPolicyFake.Execute(A<Action>._, A<Action<Exception>>._))
                .MustNotHaveHappened();
        }

        #endregion

        #region Negative Case: Handler Throws and Goes to DeadLetter

        [Fact]
        public void Dispatch_ShouldSendToDeadLetter_WhenHandlerFailsAfterRetries()
        {
            var handlerFake = A.Fake<IEventHandler>();
            A.CallTo(() => handlerFake.Topic).Returns("user.failed");

            var handlers = new List<IEventHandler> { handlerFake };

            //  FIXED: Fake GetService instead of GetServices<T>()
            A.CallTo(() => _providerFake.GetService(typeof(IEnumerable<IEventHandler>)))
                .Returns(handlers);

            var testException = new Exception("Boom!");

            // Simulate retry failure
            A.CallTo(() => _retryPolicyFake.Execute(A<Action>._, A<Action<Exception>>._))
                .Invokes((Action action, Action<Exception> onFailure) => onFailure(testException));

            using var console = new ConsoleCapture();

            // Act
            _dispatcher.Dispatch("user.failed", "{ \"Name\": \"Bob\" }");

            // Assert
            var output = console.GetOutput();
            Assert.Contains("Failed processing", output);

            A.CallTo(() => _deadLetterProducerFake.Send(
                A<string>.That.Contains("Bob"),
                A<string>.That.Contains("Boom!")
            )).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region Helper

        private class ConsoleCapture : IDisposable
        {
            private readonly System.IO.StringWriter _writer;
            private readonly System.IO.TextWriter _originalOut;

            public ConsoleCapture()
            {
                _writer = new System.IO.StringWriter();
                _originalOut = Console.Out;
                Console.SetOut(_writer);
            }

            public string GetOutput() => _writer.ToString();

            public void Dispose()
            {
                Console.SetOut(_originalOut);
                _writer.Dispose();
            }
        }

        #endregion
    }
}
