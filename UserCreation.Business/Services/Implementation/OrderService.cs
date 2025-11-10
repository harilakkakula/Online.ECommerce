using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserCreation.Business.Constants;
using UserCreation.Business.Data.Repository;
using UserCreation.Business.Entities;
using UserCreation.Business.Events;
using UserCreation.Business.Services.Interface;

namespace UserCreation.Business.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<RefOrder> _repository;
        private readonly ILogger<OrderService> _logger;
        private readonly IUserService _userService;
        public OrderService(
            IRepository<RefOrder> repository,
            ILogger<OrderService> logger,
            IUserService userService)
        {
            _repository = repository;
            _logger = logger;
            _userService = userService;
        }
        public async Task CreateOrderAsync(OrderCreatedEvent request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                //  Check if User exists
                var user = await _userService.GetUserByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning(AppMessages.UserNotFound, request.UserId);
                    throw new InvalidOperationException(string.Format(AppMessages.UserNotFound, request.UserId));
                }

                //  Save Order
                await SaveOrderAsync(request);

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

        private async Task<RefOrder> SaveOrderAsync(OrderCreatedEvent request)
        {
            var order = new RefOrder(
                request.Id,
                request.UserId,
                request.Product,
                request.Quantity,
                request.Price);

            await _repository.AddAsync(order);
            _logger.LogInformation(AppMessages.OrderCreatedSuccess, order.Id);
            return order;
        }

        public async Task<RefOrder?> GetOrderByIdAsync(Guid id)
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
