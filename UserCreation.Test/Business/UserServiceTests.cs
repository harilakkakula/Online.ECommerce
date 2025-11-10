using Common.Integration;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserCreation.Business.Data.Repository;
using UserCreation.Business.Dto;
using UserCreation.Business.Entities;
using UserCreation.Business.Services.Implementation;

namespace UserCreation.Test.Business
{
    public class UserServiceTests
    {
        #region Setup

        private readonly IRepository<User> _repositoryFake;
        private readonly IRepository<RefOrder> _refOrderRepositoryFake;
        private readonly IProducerService _producerFake;
        private readonly ILogger<UserService> _loggerFake;
        private readonly IConfiguration _configurationFake;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _repositoryFake = A.Fake<IRepository<User>>();
            _refOrderRepositoryFake = A.Fake<IRepository<RefOrder>>();
            _producerFake = A.Fake<IProducerService>();
            _loggerFake = A.Fake<ILogger<UserService>>();

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "KafkaTopics:UserCreated", "user.created" }
            };

            _configurationFake = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _service = new UserService(_repositoryFake, _refOrderRepositoryFake, _producerFake, _configurationFake, _loggerFake);
        }

        #endregion

        #region CreateUserAsync Tests

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser_WhenValidRequest()
        {
            #region Arrange
            var request = new UserCertationDto { Name = "Alice", Email = "alice@example.com" };

            A.CallTo(() => _repositoryFake.FindAsync(A<System.Linq.Expressions.Expression<Func<User, bool>>>._))
                .Returns(Task.FromResult<IEnumerable<User>>(new List<User>()));
            #endregion

            #region Act
            var result = await _service.CreateUserAsync(request);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal("Alice", result.Name);
            A.CallTo(() => _repositoryFake.AddAsync(A<User>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _producerFake.ProduceAsync("user.created", A<User>._)).MustHaveHappenedOnceExactly();
            #endregion
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            #region Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateUserAsync(null!));
            #endregion
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowInvalidOperationException_WhenEmailExists()
        {
            #region Arrange
            var request = new UserCertationDto { Name = "Bob", Email = "bob@example.com" };
            var existingUser = new User(Guid.NewGuid(), "Bob", "bob@example.com");

            A.CallTo(() => _repositoryFake.FindAsync(A<System.Linq.Expressions.Expression<Func<User, bool>>>._))
                .Returns(Task.FromResult<IEnumerable<User>>(new List<User> { existingUser }));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateUserAsync(request));
            #endregion
        }

        [Fact]
        public async Task CreateUserAsync_ShouldLogError_WhenUnexpectedExceptionOccurs()
        {
            #region Arrange
            var request = new UserCertationDto { Name = "Error", Email = "error@example.com" };

            A.CallTo(() => _repositoryFake.FindAsync(A<System.Linq.Expressions.Expression<Func<User, bool>>>._))
                .Throws(new Exception("DB error"));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CreateUserAsync(request));

            A.CallTo(_loggerFake)
                .Where(call => call.Method.Name == "Log")
                .MustHaveHappened();
            #endregion
        }

        #endregion

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnPagedResults()
        {
            #region Arrange
            var users = Enumerable.Range(1, 15)
                .Select(i => new User(Guid.NewGuid(), $"User{i}", $"user{i}@test.com"))
                .ToList();

            var orders = users.Take(10)
                .Select(u => new RefOrder(
                    Guid.NewGuid(),
                    u.Id,
                    $"Product-{u.Name}",
                    1,
                    100
                )).ToList();

            A.CallTo(() => _repositoryFake.GetAllAsync())
                .Returns(Task.FromResult<IEnumerable<User>>(users));

            A.CallTo(() => _refOrderRepositoryFake.GetAllAsync())
                .Returns(Task.FromResult<IEnumerable<RefOrder>>(orders));
            #endregion

            #region Act
            var result = await _service.GetAllUsersAsync(2, 5);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count());
            Assert.IsAssignableFrom<UserWithOrdersViewModel>(result.First());
            #endregion
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldThrowException_WhenRepositoryFails()
        {
            #region Arrange
            A.CallTo(() => _repositoryFake.GetAllAsync()).Throws(new Exception("DB fail"));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetAllUsersAsync());
            #endregion
        }

        #endregion

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUserWithOrders_WhenExists()
        {
            #region Arrange
            var userId = Guid.NewGuid();
            var user = new User(userId, "Daisy", "daisy@test.com");
            var orders = new List<RefOrder>
            {
                new RefOrder(
                    Guid.NewGuid(),
                    userId,
                    "Laptop",
                    1,
                    1500
                )
            };

            A.CallTo(() => _repositoryFake.GetByIdAsync(userId))
                .Returns(Task.FromResult(user));

            A.CallTo(() => _refOrderRepositoryFake.FindAsync(A<System.Linq.Expressions.Expression<Func<RefOrder, bool>>>._))
                .Returns(Task.FromResult<IEnumerable<RefOrder>>(orders));
            #endregion

            #region Act
            var result = await _service.GetUserByIdAsync(userId);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal("Daisy", result.Name);
            Assert.Single(result.Orders);
            Assert.Equal("Laptop", result.Orders.First().Product);
            #endregion
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldThrowKeyNotFoundException_WhenUserNotFound()
        {
            #region Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _repositoryFake.GetByIdAsync(id))
                .Returns(Task.FromResult<User?>(null));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetUserByIdAsync(id));
            #endregion
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldThrowException_WhenRepositoryFails()
        {
            #region Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _repositoryFake.GetByIdAsync(id))
                .Throws(new Exception("DB crash"));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetUserByIdAsync(id));
            #endregion
        }

        #endregion
    }
}
