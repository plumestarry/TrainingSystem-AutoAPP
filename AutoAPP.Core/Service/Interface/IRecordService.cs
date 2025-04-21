using AutoShared;
using AutoShared.Dtos;
using AutoShared.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAPP.Core.Service.Interface
{
    public interface IRecordService : IBaseService<RecordDto>
    {
        Task<ApiResponse<PagedList<RecordDto>>> GetAllFilterAsync(QueryParameter parameter);

        Task<ApiResponse<SummaryDto>> SummaryAsync();
    }
}
