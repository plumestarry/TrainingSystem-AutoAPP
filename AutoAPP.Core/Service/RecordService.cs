using AutoAPP.Core.Service.Client;
using AutoAPP.Core.Service.Interface;
using AutoShared;
using AutoShared.Dtos;
using AutoShared.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAPP.Core.Service
{
    public class RecordService(HttpRestClient client) : BaseService<RecordDto>(client, "Record"), IRecordService
    {
        private readonly HttpRestClient client = client;

        public async Task<ApiResponse<List<RecordDto>>> GetAllFilterAsync(string userName)
        {
            BaseRequest request = new BaseRequest();
            request.Method = RestSharp.Method.Get;
            request.Route = $"api/Record/GetAll?userName={userName}";
            return await client.ExecuteAsync<List<RecordDto>>(request);
        }

        public async Task<ApiResponse<SummaryDto>> SummaryAsync()
        {
            BaseRequest request = new BaseRequest();
            request.Route = "api/Record/Summary";
            return await client.ExecuteAsync<SummaryDto>(request);
        }
    }
}
