using Common.Messaging.Kafka.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderCreation.Business.Constants;
using OrderCreation.Business.Events;
using OrderCreation.Business.Services.Interface;
using System;
using System.Text.Json;

namespace OrderCreation.EventHandlers
{
    public class UserCreatedEventHandler : IEventHandler
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserCreatedEventHandler> _logger;
        private readonly string _userCreatedTopic;

        public UserCreatedEventHandler(
            IUserService userService,
            ILogger<UserCreatedEventHandler> logger,
            IConfiguration configuration)
        {
            _userService = userService;
            _logger = logger;
            _userCreatedTopic = configuration["KafkaTopics:UserCreated"] ?? "user.created";
        }

        public string Topic => _userCreatedTopic;

        public void Handle(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning(AppMessages.EmptyEventMessage, Topic);
                return;
            }

            try
            {
                var evt = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                if (evt == null)
                {
                    _logger.LogWarning(AppMessages.InvalidEventPayload, Topic);
                    return;
                }

                _logger.LogInformation(AppMessages.EventProcessingStarted, evt.Id, evt.Name, Topic);

                // Call async service synchronously
                _userService.CreateUserAsync(evt).GetAwaiter().GetResult();

                _logger.LogInformation(AppMessages.EventProcessingSuccess, evt.Id, Topic);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, AppMessages.EventDeserializationError, Topic);
            }
            catch (InvalidOperationException opEx)
            {
                _logger.LogWarning(opEx, AppMessages.UserAlreadyExists, Topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.EventProcessingError, Topic);
            }
        }

    }
}
