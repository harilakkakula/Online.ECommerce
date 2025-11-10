using Common.Integration;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderCreation.Business.Data.Repository;
using OrderCreation.Business.Dto;
using OrderCreation.Business.Entities;
using OrderCreation.Business.Services.Implementation;
using OrderCreation.Business.Services.Interface;

namespace OrderCreation.Test.Business
{
    public class OrderServiceTests
    {
        #region Private Fields

        private readonly IRepository<Order> _orderRepositoryFake;
        private readonly IProducerService _producerFake;
        private readonly ILogger<OrderService> _loggerFake;
        private readonly IUserService _userServiceFake;
        private readonly IConfiguration _configFake;
        private readonly OrderService _service;

        #endregion

        #region Constructor

        public OrderServiceTests()
        {
            _orderRepositoryFake = A.Fake<IRepository<Order>>();
            _producerFake = A.Fake<IProducerService>();
            _loggerFake = A.Fake<ILogger<OrderService>>();
            _userServiceFake = A.Fake<IUserService>();

            var configDict = new Dictionary<string, string>
            {
                { "KafkaTopics:OrderCreated", "order.created" }
            };

            _configFake = new ConfigurationBuilder().AddInMemoryCollection(configDict!).Build();

            _service = new OrderService(
                _orderRepositoryFake,
                _producerFake,
                _configFake,
                _loggerFake,
                _userServiceFake);
        }

        #endregion

        #region CreateOrderAsync Tests

        [Fact]
        public async Task CreateOrderAsync_ShouldCreateOrder_WhenValidRequestAndUserExists()
        {
            #region Arrange
            var userId = Guid.NewGuid();
            var request = new OrderCreationDto
            {
                UserId = userId,
                Product = "Laptop",
                Quantity = 2,
                Price = 1200
            };

            // Correct: return an actual RefUser
            var fakeUser = new RefUser(userId, "Alice", "alice@example.com");

            A.CallTo(() => _userServiceFake.GetUserByIdAsync(userId))
                .Returns(Task.FromResult(fakeUser));
            #endregion

            #region Act
            var result = await _service.CreateOrderAsync(request);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("Laptop", result.Product);

            A.CallTo(() => _orderRepositoryFake.AddAsync(A<Order>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _producerFake.ProduceAsync("order.created", A<Order>._)).MustHaveHappenedOnceExactly();
            #endregion
        }


        [Fact]
        public async Task CreateOrderAsync_ShouldThrowInvalidOperationException_WhenUserNotFound()
        {
            #region Arrange
            var request = new OrderCreationDto
            {
                UserId = Guid.NewGuid(),
                Product = "Phone",
                Quantity = 1,
                Price = 800
            };

            A.CallTo(() => _userServiceFake.GetUserByIdAsync(request.UserId))
                .Returns(Task.FromResult<RefUser?>(null));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateOrderAsync(request));
            #endregion
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowValidationException_WhenInvalidDto()
        {
            #region Arrange
            var request = new OrderCreationDto
            {
                UserId = Guid.Empty,
                Product = "",
                Quantity = 0,
                Price = -10
            };
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _service.CreateOrderAsync(request));
            #endregion
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowException_WhenRepositoryFails()
        {
            #region Arrange
            var userId = Guid.NewGuid();
            var request = new OrderCreationDto
            {
                UserId = userId,
                Product = "Mouse",
                Quantity = 1,
                Price = 50
            };

            // Correct: return a valid RefUser object instead of an anonymous type
            var fakeUser = new RefUser(userId, "Alice", "alice@example.com");

            A.CallTo(() => _userServiceFake.GetUserByIdAsync(userId))
                .Returns(Task.FromResult(fakeUser));

            // Simulate repository failure
            A.CallTo(() => _orderRepositoryFake.AddAsync(A<Order>._))
                .ThrowsAsync(new Exception("Database failure"));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CreateOrderAsync(request));
            #endregion
        }


        #endregion

        #region GetOrderByIdAsync Tests

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnOrder_WhenFound()
        {
            #region Arrange
            var orderId = Guid.NewGuid();
            var order = new Order(orderId, Guid.NewGuid(), "Keyboard", 1, 100);
            A.CallTo(() => _orderRepositoryFake.GetByIdAsync(orderId))
                .Returns(Task.FromResult(order));
            #endregion

            #region Act
            var result = await _service.GetOrderByIdAsync(orderId);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal("Keyboard", result.Product);
            #endregion
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldThrowKeyNotFoundException_WhenNotFound()
        {
            #region Arrange
            var orderId = Guid.NewGuid();
            A.CallTo(() => _orderRepositoryFake.GetByIdAsync(orderId))
                .Returns(Task.FromResult<Order?>(null));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetOrderByIdAsync(orderId));
            #endregion
        }

        #endregion

        #region GetAllOrdersAsync Tests

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnPaginatedResults()
        {
            #region Arrange
            var orders = Enumerable.Range(1, 15)
                .Select(i => new Order(Guid.NewGuid(), Guid.NewGuid(), $"Product{i}", i, i * 10m))
                .ToList();

            A.CallTo(() => _orderRepositoryFake.GetAllAsync())
                .Returns(Task.FromResult<IEnumerable<Order>>(orders));
            #endregion

            #region Act
            var result = await _service.GetAllOrdersAsync(pageNumber: 2, pageSize: 5);
            #endregion

            #region Assert
            Assert.Equal(5, result.Count());
            Assert.Equal("Product6", result.First().Product);
            #endregion
        }

        #endregion
    }
}
