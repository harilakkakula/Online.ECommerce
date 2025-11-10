using OrderCreation.Business.Entities;
using OrderCreation.Business.Events;
using System;
using System.Threading.Tasks;

namespace OrderCreation.Business.Services.Interface
{
    public interface IUserService
    {
        Task CreateUserAsync(UserCreatedEvent request);
        Task<RefUser?> GetUserByIdAsync(Guid id);
    }
}
