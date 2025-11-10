using OrderCreation.Business.Dto;
using OrderCreation.Business.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace OrderCreation.Business.Services.Interface
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(OrderCreationDto request);
        Task<IEnumerable<Order>> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 10);
        Task<Order?> GetOrderByIdAsync(Guid id);
    }
}
