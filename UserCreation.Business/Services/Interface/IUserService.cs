using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserCreation.Business.Dto;
using UserCreation.Business.Entities;

namespace UserCreation.Business.Services.Interface
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(UserCertationDto request);
        Task<IEnumerable<UserWithOrdersViewModel>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10);
        Task<UserWithOrdersViewModel> GetUserByIdAsync(Guid id);
    }
}
