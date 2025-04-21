using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoShared;

namespace AutoAPP.Core.Service.Client
{
    public class HttpRestClient(string apiUrl)
    {
        private readonly string apiUrl = apiUrl;
        protected readonly RestClient client = new RestClient();

        public async Task<ApiResponse> ExecuteAsync(BaseRequest baseRequest)
        {
            var request = new RestRequest(apiUrl + baseRequest.Route, baseRequest.Method);
            request.AddHeader("Content-Type", baseRequest.ContentType);

            if (baseRequest.Parameter != null)
                //request.AddParameter("Parameters", JsonConvert.SerializeObject(baseRequest.Parameter), ParameterType.RequestBody);
                request.AddJsonBody(baseRequest.Parameter); // 使用 AddJsonBody 方法

            //client.BaseUrl = new Uri(apiUrl + baseRequest.Route);
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<ApiResponse>(response.Content);

            else
                return new ApiResponse()
                {
                    Status = false,
                    Result = null,
                    Message = response.ErrorMessage
                };
        }

        public async Task<ApiResponse<T>> ExecuteAsync<T>(BaseRequest baseRequest)
        {
            var request = new RestRequest(apiUrl + baseRequest.Route, baseRequest.Method);
            request.AddHeader("Content-Type", baseRequest.ContentType);

            if (baseRequest.Parameter != null)
                //request.AddParameter("model", JsonConvert.SerializeObject(baseRequest.Parameter), ParameterType.RequestBody);
                request.AddJsonBody(baseRequest.Parameter); // 使用 AddJsonBody 方法

            //client.BaseUrl = new Uri(apiUrl + baseRequest.Route);
            var response = await client.ExecuteAsync(request); 
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<ApiResponse<T>>(response.Content);

            else
                return new ApiResponse<T>()
                {
                    Status = false, 
                    Message = response.ErrorMessage
                };
        }
    }
}
