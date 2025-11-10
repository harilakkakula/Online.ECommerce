using FakeItEasy;
using Microsoft.Extensions.Logging;
using UserCreation.Business.Data.Repository;
using UserCreation.Business.Dto;
using UserCreation.Business.Entities;
using UserCreation.Business.Events;
using UserCreation.Business.Services.Implementation;
using UserCreation.Business.Services.Interface;

namespace UserCreation.Test.Business
{
    public class OrderServiceTests
    {
        #region Setup

        private readonly IRepository<RefOrder> _orderRepositoryFake;
        private readonly ILogger<OrderService> _loggerFake;
        private readonly IUserService _userServiceFake;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _orderRepositoryFake = A.Fake<IRepository<RefOrder>>();
            _loggerFake = A.Fake<ILogger<OrderService>>();
            _userServiceFake = A.Fake<IUserService>();

            _service = new OrderService(_orderRepositoryFake, _loggerFake, _userServiceFake);
        }

        #endregion

        #region CreateOrderAsync Tests

        [Fact]
        public async Task CreateOrderAsync_ShouldCreateOrder_WhenValidRequestAndUserExists()
        {
            #region Arrange
            var userId = Guid.NewGuid();
            var request = new OrderCreatedEvent
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Product = "Laptop",
                Quantity = 2,
                Price = 1200
            };

            var fakeUser = new UserWithOrdersViewModel
            {
                UserId = userId,
                Name = "Alice",
                Email = "alice@example.com",
                Orders = new List<OrderViewModel>()
            };

            A.CallTo(() => _userServiceFake.GetUserByIdAsync(userId))
                .Returns(Task.FromResult<UserWithOrdersViewModel?>(fakeUser));
            #endregion

            #region Act
            await _service.CreateOrderAsync(request);
            #endregion

            #region Assert
            A.CallTo(() => _orderRepositoryFake.AddAsync(A<RefOrder>._))
                .MustHaveHappenedOnceExactly();
            #endregion
        }


        [Fact]
        public async Task CreateOrderAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            #region Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateOrderAsync(null!));
            #endregion
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowInvalidOperationException_WhenUserNotFound()
        {
            #region Arrange
            var request = new OrderCreatedEvent
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Product = "Phone",
                Quantity = 1,
                Price = 800
            };

            A.CallTo(() => _userServiceFake.GetUserByIdAsync(request.UserId))
                .Returns(Task.FromResult<UserWithOrdersViewModel?>(null));
            #endregion

            #region Act
            await Assert.ThrowsAsync<FormatException>(() => _service.CreateOrderAsync(request));
            #endregion
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldLogError_WhenUnexpectedExceptionOccurs()
        {
            #region Arrange
            var request = new OrderCreatedEvent
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Product = "Tablet",
                Quantity = 3,
                Price = 500
            };

            A.CallTo(() => _userServiceFake.GetUserByIdAsync(request.UserId))
                .Throws(new Exception("DB Failure"));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CreateOrderAsync(request));

            A.CallTo(_loggerFake)
                .Where(call => call.Method.Name == "Log")
                .MustHaveHappened();
            #endregion
        }

        #endregion

        #region GetOrderByIdAsync Tests

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnOrder_WhenExists()
        {
            #region Arrange
            var orderId = Guid.NewGuid();
            var order = new RefOrder(orderId, Guid.NewGuid(), "Keyboard", 1, 100);

            A.CallTo(() => _orderRepositoryFake.GetByIdAsync(orderId))
                .Returns(Task.FromResult(order));
            #endregion

            #region Act
            var result = await _service.GetOrderByIdAsync(orderId);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result!.Id);
            Assert.Equal("Keyboard", result.Product);
            #endregion
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldThrowKeyNotFoundException_WhenOrderNotFound()
        {
            #region Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _orderRepositoryFake.GetByIdAsync(id))
                .Returns(Task.FromResult<RefOrder?>(null));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetOrderByIdAsync(id));
            #endregion
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldThrowException_WhenRepositoryFails()
        {
            #region Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _orderRepositoryFake.GetByIdAsync(id))
                .Throws(new Exception("DB crash"));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetOrderByIdAsync(id));
            A.CallTo(_loggerFake)
                .Where(call => call.Method.Name == "Log")
                .MustHaveHappened();
            #endregion
        }

        #endregion
    }
}
