using Common.Messaging.Kafka.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using UserCreation.Business.Constants;
using UserCreation.Business.Events;
using UserCreation.Business.Services.Interface;

namespace UserCreation.EventHandlers
{
    public class OrderCreatedEventHandler : IEventHandler
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly ILogger<OrderCreatedEventHandler> _logger;
        private readonly string _orderCreatedTopic;

        #endregion

        #region Constructor

        public OrderCreatedEventHandler(
            IOrderService orderService,
            ILogger<OrderCreatedEventHandler> logger,
            IConfiguration configuration)
        {
            _orderService = orderService;
            _logger = logger;
            _orderCreatedTopic = configuration["KafkaTopics:OrderCreated"] ?? "order.created";
        }

        #endregion

        #region Properties

        public string Topic => _orderCreatedTopic;

        #endregion

        #region Methods

        public void Handle(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning(AppMessages.EmptyEventMessage, Topic);
                return;
            }

            try
            {
                // Deserialize event
                var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(message);

                if (evt == null)
                {
                    _logger.LogWarning(AppMessages.InvalidEventPayload, Topic);
                    return;
                }

                _logger.LogInformation(AppMessages.EventProcessingStarted, evt.Id, evt.Product, Topic);
                _orderService.CreateOrderAsync(evt).GetAwaiter().GetResult();

                _logger.LogInformation(AppMessages.EventProcessingSuccess, evt.Id, Topic);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, AppMessages.EventDeserializationError, Topic);
            }
            catch (InvalidOperationException opEx)
            {
                _logger.LogWarning(opEx, AppMessages.OrderAlreadyExists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.EventProcessingError, Topic);
            }
        }

        #endregion
    }
}
