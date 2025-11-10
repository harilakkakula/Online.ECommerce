using FakeItEasy;
using Microsoft.Extensions.Logging;
using OrderCreation.Business.Data.Repository;
using OrderCreation.Business.Entities;
using OrderCreation.Business.Events;
using OrderCreation.Business.Services.Implementation;

namespace OrderCreation.Test.Business
{
    public class UserServiceTests
    {
        private readonly IRepository<RefUser> _userRepositoryFake;
        private readonly ILogger<UserService> _loggerFake;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepositoryFake = A.Fake<IRepository<RefUser>>();
            _loggerFake = A.Fake<ILogger<UserService>>();
            _service = new UserService(_userRepositoryFake, _loggerFake);
        }

        #region CreateUserAsync

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser_WhenValidRequest()
        {
            #region Arrange
            var request = new UserCreatedEvent
            {
                Id = Guid.NewGuid(),
                Name = "Alice",
                Email = "alice@example.com"
            };

            A.CallTo(() => _userRepositoryFake.FindAsync(
                A<System.Linq.Expressions.Expression<Func<RefUser, bool>>>._))
                .Returns(Task.FromResult<IEnumerable<RefUser>>(new List<RefUser>()));
            #endregion

            #region Act
            await _service.CreateUserAsync(request);
            #endregion

            #region Assert
            A.CallTo(() => _userRepositoryFake.AddAsync(A<RefUser>._))
                .MustHaveHappenedOnceExactly();
            #endregion
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
        {
            #region Arrange
            var request = new UserCreatedEvent
            {
                Id = Guid.NewGuid(),
                Name = "Bob",
                Email = "bob@example.com"
            };

            var existingUser = new RefUser(Guid.NewGuid(), "Existing Bob", "bob@example.com");

            A.CallTo(() => _userRepositoryFake.FindAsync(
                A<System.Linq.Expressions.Expression<Func<RefUser, bool>>>._))
                .Returns(Task.FromResult<IEnumerable<RefUser>>(new List<RefUser> { existingUser }));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateUserAsync(request));
            #endregion
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenRepositoryFails()
        {
            #region Arrange
            var request = new UserCreatedEvent
            {
                Id = Guid.NewGuid(),
                Name = "Charlie",
                Email = "charlie@example.com"
            };

            A.CallTo(() => _userRepositoryFake.FindAsync(
                A<System.Linq.Expressions.Expression<Func<RefUser, bool>>>._))
                .Returns(Task.FromResult<IEnumerable<RefUser>>(new List<RefUser>()));

            A.CallTo(() => _userRepositoryFake.AddAsync(A<RefUser>._))
                .ThrowsAsync(new Exception("Database error"));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CreateUserAsync(request));
            #endregion
        }

        #endregion

        #region GetUserByIdAsync

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            #region Arrange
            var userId = Guid.NewGuid();
            var user = new RefUser(userId, "David", "david@example.com");

            A.CallTo(() => _userRepositoryFake.GetByIdAsync(userId))
                .Returns(Task.FromResult<RefUser?>(user));
            #endregion

            #region Act
            var result = await _service.GetUserByIdAsync(userId);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal("David", result?.Name);
            #endregion
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldThrowInvalidOperationException_WhenUserNotFound()
        {
            #region Arrange
            var userId = Guid.NewGuid();

            A.CallTo(() => _userRepositoryFake.GetByIdAsync(userId))
                .Returns(Task.FromResult<RefUser?>(null));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetUserByIdAsync(userId));
            #endregion
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldThrowException_WhenRepositoryFails()
        {
            #region Arrange
            var userId = Guid.NewGuid();

            A.CallTo(() => _userRepositoryFake.GetByIdAsync(userId))
                .ThrowsAsync(new Exception("Database failure"));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetUserByIdAsync(userId));
            #endregion
        }

        #endregion
    }
}
