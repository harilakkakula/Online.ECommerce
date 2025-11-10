using Common.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderCreation.Business.Constants;
using OrderCreation.Business.Dto;
using OrderCreation.Business.Entities;
using OrderCreation.Business.Services.Interface;
using OrderCreation.Business.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using OrderCreation.Business.Data.Repository;


namespace OrderCreation.Business.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _repository;
        private readonly IProducerService _producer;
        private readonly ILogger<OrderService> _logger;
        private readonly IUserService _userService;
        private readonly string _orderCreatedTopic;
        private readonly OrderCreationDtoValidator _validator;

        public OrderService(
            IRepository<Order> repository,
            IProducerService producer,
            IConfiguration configuration,
            ILogger<OrderService> logger,
            IUserService userService)
        {
            _repository = repository;
            _producer = producer;
            _logger = logger;
            _userService = userService;
            _validator = new OrderCreationDtoValidator();
            _orderCreatedTopic = configuration["KafkaTopics:OrderCreated"] ?? "order.created";
        }

        public async Task<Order> CreateOrderAsync(OrderCreationDto request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                // Validate DTO
                var validationResult = _validator.Validate(request);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    throw new ValidationException($"Order validation failed: {errors}");
                }

                //  Check if User exists
                var user = await _userService.GetUserByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning(AppMessages.UserNotFound, request.UserId);
                    throw new InvalidOperationException(string.Format(AppMessages.UserNotFound, request.UserId));
                }

                //  Save Order
                var order = await SaveOrderAsync(request);

                //  Publish to Kafka
                await _producer.ProduceAsync(_orderCreatedTopic, order);
                _logger.LogInformation(AppMessages.OrderEventPublished, _orderCreatedTopic);

                return order;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.UnexpectedErrorCreatingOrder);
                throw;
            }
        }

        private async Task<Order> SaveOrderAsync(OrderCreationDto request)
        {
            var order = new Order(
                Guid.NewGuid(),
                request.UserId,
                request.Product,
                request.Quantity,
                request.Price);

            await _repository.AddAsync(order);
            _logger.LogInformation(AppMessages.OrderCreatedSuccess, order.Id);
            return order;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var orders = await _repository.GetAllAsync();
                return orders
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.ErrorRetrievingOrders);
                throw;
            }
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            try
            {
                var order = await _repository.GetByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning(AppMessages.OrderNotFound, id);
                    throw new KeyNotFoundException(string.Format(AppMessages.NotFound, "Order", id));
                }

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.ErrorGettingOrderById, id);
                throw;
            }
        }
    }
}
