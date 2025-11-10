using Microsoft.Extensions.Logging;
using OrderCreation.Business.Constants;
using OrderCreation.Business.Data.Repository;
using OrderCreation.Business.Entities;
using OrderCreation.Business.Events;
using OrderCreation.Business.Services.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrderCreation.Business.Services.Implementation
{
    public class UserService: IUserService
    {
        private readonly IRepository<RefUser> _repository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IRepository<RefUser> repository,
            ILogger<UserService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task CreateUserAsync(UserCreatedEvent request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                // Check if user with the same email already exists
                await DoesEmailExistAsync(request);

                //Save User
                var user = await SaveUserAsync(request);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.UnexpectedErrorCreatingUser);
                throw;
            }
        }

        private async Task<RefUser> SaveUserAsync(UserCreatedEvent request)
        {
            var user = new RefUser(request.Id, request.Name, request.Email);
            await _repository.AddAsync(user);
            _logger.LogInformation(AppMessages.UserCreatedSuccess, user.Id);
            return user;
        }

        private async Task DoesEmailExistAsync(UserCreatedEvent request)
        {
            var existingUsers = await _repository.FindAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (existingUsers.Any())
            {
                _logger.LogWarning(AppMessages.UserAlreadyExists, request.Email);
                throw new InvalidOperationException(string.Format(AppMessages.UserAlreadyExists, request.Email));
            }
        }

        public async Task<RefUser?> GetUserByIdAsync(Guid id)
        {
            try
            {
                var user = await _repository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning(AppMessages.UserNotFound, id);
                    throw new InvalidOperationException(string.Format(AppMessages.NotFound, "User", id));
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.ErrorGettingUserById, id);
                throw;
            }
        }
    }
}
