using AutoAPI.Service.Response;
using AutoShared.Parameters;

namespace AutoAPI.Service.Interface
{
    public interface IBaseService<T>
    {
        Task<ApiResponse> GetAllAsync(string userName);

        Task<ApiResponse> GetSingleAsync(string userName);

        Task<ApiResponse> AddAsync(T model);

        Task<ApiResponse> UpdateAsync(T model);

        Task<ApiResponse> DeleteAsync(int id);
    }
}
