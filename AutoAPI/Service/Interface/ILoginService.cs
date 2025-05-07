using AutoAPI.Service.Response;
using AutoShared.Dtos;

namespace AutoAPI.Service.Interface
{
    public interface ILoginService
    {
        Task<ApiResponse> LoginAsync(string Account, string Password);

        Task<ApiResponse> Register(UserDto user);

        Task<ApiResponse> GetAllAsync();
    }
}
