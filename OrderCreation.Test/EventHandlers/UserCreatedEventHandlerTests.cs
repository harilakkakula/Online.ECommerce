using System.Text.Json;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderCreation.Business.Events;
using OrderCreation.Business.Services.Interface;
using OrderCreation.EventHandlers;

namespace OrderCreation.Test.EventHandlers
{
    public class UserCreatedEventHandlerTests
    {
        #region Fields

        private readonly IUserService _userService;
        private readonly ILogger<UserCreatedEventHandler> _logger;
        private readonly IConfiguration _configuration;
        private readonly UserCreatedEventHandler _handler;

        #endregion

        #region Constructor

        public UserCreatedEventHandlerTests()
        {
            #region Arrange
            _userService = A.Fake<IUserService>();
            _logger = A.Fake<ILogger<UserCreatedEventHandler>>();

            var settings = new Dictionary<string, string?>
            {
                { "KafkaTopics:UserCreated", "user.created" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            _handler = new UserCreatedEventHandler(_userService, _logger, _configuration);
            #endregion
        }

        #endregion

        #region Topic Tests

        [Fact]
        public void Topic_ShouldReturnConfiguredTopic()
        {
            #region Assert
            Assert.Equal("user.created", _handler.Topic);
            #endregion
        }

        #endregion

        #region Negative Cases

        [Fact]
        public void Handle_ShouldSkip_WhenMessageIsEmpty()
        {
            #region Arrange
            var message = string.Empty;
            #endregion

            #region Act
            _handler.Handle(message);
            #endregion

            #region Assert
            A.CallTo(() => _userService.CreateUserAsync(A<UserCreatedEvent>._))
                .MustNotHaveHappened();
            #endregion
        }

        [Fact]
        public void Handle_ShouldSkip_WhenInvalidJson()
        {
            #region Arrange
            var invalidJson = "{ invalid json }";
            #endregion

            #region Act
            _handler.Handle(invalidJson);
            #endregion

            #region Assert
            A.CallTo(() => _userService.CreateUserAsync(A<UserCreatedEvent>._))
                .MustNotHaveHappened();
            #endregion
        }

        [Fact]
        public void Handle_ShouldSkip_WhenEventIsNull()
        {
            #region Arrange
            var message = "null";
            #endregion

            #region Act
            _handler.Handle(message);
            #endregion

            #region Assert
            A.CallTo(() => _userService.CreateUserAsync(A<UserCreatedEvent>._))
                .MustNotHaveHappened();
            #endregion
        }

        [Fact]
        public void Handle_ShouldContinue_WhenUserAlreadyExists()
        {
            #region Arrange
            var evt = new UserCreatedEvent { Id = Guid.NewGuid(), Name = "Bob" };
            var json = JsonSerializer.Serialize(evt);

            A.CallTo(() => _userService.CreateUserAsync(A<UserCreatedEvent>._))
                .Throws(new InvalidOperationException("User already exists"));
            #endregion

            #region Act
            var exception = Record.Exception(() => _handler.Handle(json));
            #endregion

            #region Assert
            Assert.Null(exception);
            #endregion
        }

        [Fact]
        public void Handle_ShouldContinue_WhenUnexpectedErrorOccurs()
        {
            #region Arrange
            var evt = new UserCreatedEvent { Id = Guid.NewGuid(), Name = "Charlie" };
            var json = JsonSerializer.Serialize(evt);

            A.CallTo(() => _userService.CreateUserAsync(A<UserCreatedEvent>._))
                .Throws(new Exception("Unexpected error"));
            #endregion

            #region Act
            var exception = Record.Exception(() => _handler.Handle(json));
            #endregion

            #region Assert
            Assert.Null(exception);
            #endregion
        }

        #endregion

        #region Positive Cases

        [Fact]
        public void Handle_ShouldCallUserService_WhenValidMessage()
        {
            #region Arrange
            var evt = new UserCreatedEvent
            {
                Id = Guid.NewGuid(),
                Name = "Alice",
                Email = "alice@example.com"
            };

            var json = JsonSerializer.Serialize(evt);
            #endregion

            #region Act
            _handler.Handle(json);
            #endregion

            #region Assert
            A.CallTo(() => _userService.CreateUserAsync(A<UserCreatedEvent>.That.Matches(e =>
                e.Id == evt.Id && e.Name == evt.Name && e.Email == evt.Email)))
                .MustHaveHappenedOnceExactly();
            #endregion
        }

        #endregion
    }
}
