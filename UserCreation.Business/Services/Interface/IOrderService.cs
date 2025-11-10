using System;
using System.Threading.Tasks;
using UserCreation.Business.Entities;
using UserCreation.Business.Events;

namespace UserCreation.Business.Services.Interface
{
    public interface IOrderService
    {
        Task CreateOrderAsync(OrderCreatedEvent request);
        Task<RefOrder?> GetOrderByIdAsync(Guid id);
    }
}
