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
        Task<ApiResponse<List<RecordDto>>> GetAllFilterAsync(string userName);

        Task<ApiResponse<SummaryDto>> SummaryAsync(string userName);
    }
}
