using Common.Integration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UserCreation.Business.Dto;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Extensions.Logging;
using UserCreation.Business.Constants;
using UserCreation.Business.Entities;
using UserCreation.Business.Services.Interface;
using UserCreation.Business.Data.Repository;

namespace UserCreation.Business.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IRepository<RefOrder> _orderRepository;
        private readonly IProducerService _producer;
        private readonly ILogger<UserService> _logger;
        private readonly string _userCreatedTopic;

        public UserService(
            IRepository<User> repository,
            IRepository<RefOrder> orderRepository,
            IProducerService producer,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _repository = repository;
            _orderRepository = orderRepository;
            _producer = producer;
            _logger = logger;
            _userCreatedTopic = configuration["KafkaTopics:UserCreated"] ?? "user.created";
        }

        public async Task<User> CreateUserAsync(UserCertationDto request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                await DoesEmailExistAsync(request);

                var user = await SaveUserAsync(request);

                await _producer.ProduceAsync(_userCreatedTopic, user);
                _logger.LogInformation(AppMessages.UserEventPublished, _userCreatedTopic);

                return user;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.UnexpectedErrorCreatingUser);
                throw;
            }
        }

        private async Task<User> SaveUserAsync(UserCertationDto request)
        {
            var user = new User(Guid.NewGuid(), request.Name, request.Email);
            await _repository.AddAsync(user);
            _logger.LogInformation(AppMessages.UserCreatedSuccess, user.Id);
            return user;
        }

        private async Task DoesEmailExistAsync(UserCertationDto request)
        {
            var existingUsers = await _repository.FindAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (existingUsers.Any())
            {
                _logger.LogWarning(AppMessages.UserAlreadyExists, request.Email);
                throw new InvalidOperationException(string.Format(AppMessages.UserAlreadyExists, request.Email));
            }
        }

        /// <summary>
        /// LEFT JOIN: Get all users with their associated orders
        /// </summary>
        public async Task<IEnumerable<UserWithOrdersViewModel>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var users = await _repository.GetAllAsync();
                var orders = await _orderRepository.GetAllAsync();

                var query = from u in users
                            join o in orders
                            on u.Id equals o.UserId into userOrders
                            from order in userOrders.DefaultIfEmpty()
                            group order by u into grouped
                            select new UserWithOrdersViewModel
                            {
                                UserId = grouped.Key.Id,
                                Name = grouped.Key.Name,
                                Email = grouped.Key.Email,
                                Orders = grouped
                                    .Where(o => o != null)
                                    .Select(o => new OrderViewModel
                                    {
                                        Id = o.Id,
                                        Product = o.Product,
                                        Quantity = o.Quantity,
                                        Price = o.Price
                                    })
                                    .ToList()
                            };

                return query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.ErrorRetrievingUsers);
                throw;
            }
        }

        /// <summary>
        /// LEFT JOIN: Get single user with their associated orders
        /// </summary>
        public async Task<UserWithOrdersViewModel> GetUserByIdAsync(Guid id)
        {
            try
            {
                var user = await _repository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning(AppMessages.UserNotFound, id);
                    throw new KeyNotFoundException(string.Format(AppMessages.NotFound, "User", id));
                }

                var orders = await _orderRepository.FindAsync(o => o.UserId == id);

                var result = new UserWithOrdersViewModel
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Orders = orders.Select(o => new OrderViewModel
                    {
                        Id = o.Id,
                        Product = o.Product,
                        Quantity = o.Quantity,
                        Price = o.Price
                    }).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.ErrorGettingUserById, id);
                throw;
            }
        }
    }
}
