using System.Text.Json;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserCreation.Business.Events;
using UserCreation.Business.Services.Interface;
using UserCreation.EventHandlers;

namespace UserCreation.Test.EventHandlers
{
    public class OrderCreatedEventHandlerTests
    {
        private readonly IOrderService _orderServiceFake;
        private readonly ILogger<OrderCreatedEventHandler> _loggerFake;
        private readonly IConfiguration _configuration;
        private readonly OrderCreatedEventHandler _handler;

        public OrderCreatedEventHandlerTests()
        {
            _orderServiceFake = A.Fake<IOrderService>();
            _loggerFake = A.Fake<ILogger<OrderCreatedEventHandler>>();

            var inMemorySettings = new Dictionary<string, string>
            {
                { "KafkaTopics:OrderCreated", "order.created" }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _handler = new OrderCreatedEventHandler(_orderServiceFake, _loggerFake, _configuration);
        }

        #region ✅ Positive Test — Valid Message

        [Fact]
        public void Handle_ShouldProcessOrder_WhenMessageIsValid()
        {
            // Arrange
            var evt = new OrderCreatedEvent
            {
                Id = Guid.NewGuid(),
                Product = "Laptop",
                Quantity = 1
            };
            var message = JsonSerializer.Serialize(evt);

            // Act
            _handler.Handle(message);

            // Assert
            A.CallTo(() => _orderServiceFake.CreateOrderAsync(
                A<OrderCreatedEvent>.That.Matches(e => e.Id == evt.Id && e.Product == "Laptop")))
                .MustHaveHappenedOnceExactly();

            A.CallTo(_loggerFake)
                .Where(call => call.Method.Name == "Log" &&
                               call.GetArgument<LogLevel>(0) == LogLevel.Information)
                .MustHaveHappened();
        }

        #endregion

        #region ⚠️ Negative Test — Empty Message

        [Fact]
        public void Handle_ShouldLogWarning_WhenMessageIsEmpty()
        {
            // Act
            _handler.Handle("");

            // Assert
            A.CallTo(_loggerFake)
                .Where(call => call.Method.Name == "Log" &&
                               call.GetArgument<LogLevel>(0) == LogLevel.Warning)
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _orderServiceFake.CreateOrderAsync(A<OrderCreatedEvent>._))
                .MustNotHaveHappened();
        }

        #endregion

        #region ⚠️ Negative Test — Invalid JSON

        [Fact]
        public void Handle_ShouldLogError_WhenJsonIsInvalid()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            _handler.Handle(invalidJson);

            // Assert
            A.CallTo(_loggerFake)
                .Where(call => call.Method.Name == "Log" &&
                               call.GetArgument<LogLevel>(0) == LogLevel.Error)
                .MustHaveHappened();
        }

        #endregion

        #region ⚠️ Negative Test — Invalid Payload (Deserialized to null)

        [Fact]
        public void Handle_ShouldLogWarning_WhenEventIsNull()
        {
            // Arrange
            var nullJson = "null"; // valid JSON, but deserializes to null

            // Act
            _handler.Handle(nullJson);

            // Assert
            A.CallTo(_loggerFake)
                .Where(call => call.Method.Name == "Log" &&
                               call.GetArgument<LogLevel>(0) == LogLevel.Warning)
                .MustHaveHappened();

            A.CallTo(() => _orderServiceFake.CreateOrderAsync(A<OrderCreatedEvent>._))
                .MustNotHaveHappened();
        }

        #endregion

        #region ⚠️ Negative Test — Order Already Exists (InvalidOperationException)

        [Fact]
        public void Handle_ShouldLogWarning_WhenOrderAlreadyExists()
        {
            // Arrange
            var evt = new OrderCreatedEvent
            {
                Id = Guid.NewGuid(),
                Product = "Phone",
                Quantity = 2
            };
            var message = JsonSerializer.Serialize(evt);

            A.CallTo(() => _orderServiceFake.CreateOrderAsync(A<OrderCreatedEvent>._))
                .Throws(new InvalidOperationException("Order already exists"));

            // Act
            _handler.Handle(message);

            // Assert
            A.CallTo(_loggerFake)
                .Where(call => call.Method.Name == "Log" &&
                               call.GetArgument<LogLevel>(0) == LogLevel.Warning)
                .MustHaveHappened();
        }

        #endregion

        #region 💀 Negative Test — Unexpected Exception

        [Fact]
        public void Handle_ShouldLogError_WhenUnhandledExceptionOccurs()
        {
            // Arrange
            var evt = new OrderCreatedEvent
            {
                Id = Guid.NewGuid(),
                Product = "Tablet",
                Quantity = 3
            };
            var message = JsonSerializer.Serialize(evt);

            A.CallTo(() => _orderServiceFake.CreateOrderAsync(A<OrderCreatedEvent>._))
                .Throws(new Exception("Database unreachable"));

            // Act
            _handler.Handle(message);

            // Assert
            A.CallTo(_loggerFake)
                .Where(call => call.Method.Name == "Log" &&
                               call.GetArgument<LogLevel>(0) == LogLevel.Error)
                .MustHaveHappened();
        }

        #endregion
    }
}
