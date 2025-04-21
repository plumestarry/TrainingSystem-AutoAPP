using AutoAPI.Service.Response;
using AutoShared.Dtos;

namespace AutoAPI.Service.Interface
{
    public interface IRecordService : IBaseService<RecordDto>
    {
        Task<ApiResponse> Summary();
    }
}
