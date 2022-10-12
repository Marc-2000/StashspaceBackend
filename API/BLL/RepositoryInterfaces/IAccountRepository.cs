using API.BLL.DTOs;
using API.DAL.Context;
using API.Entities;

namespace API.BLL.RepositoryInterfaces
{
    public interface IAccountRepository
    {
        Task<ServiceResponse> Register(UserRegisterDTO user);
        Task<ServiceResponse> Login(UserLoginDTO user);
        Task<ServiceResponse> Delete(Guid userId);
        Task<User> GetByID(Guid id);
    }
}
