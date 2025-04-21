using AutoAPP.Core.Service.Client;
using AutoAPP.Core.Service.Interface;
using AutoShared;
using AutoShared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAPP.Core.Service
{
    public class LoginService(HttpRestClient client) : ILoginService
    {
        private readonly HttpRestClient client = client;
        private readonly string serviceName = "Login";

        public async Task<ApiResponse<UserDto>> Login(UserDto user)
        {
            BaseRequest request = new BaseRequest();
            request.Method = RestSharp.Method.Post;
            request.Route = $"api/{serviceName}/Login";
            request.Parameter = user;
            return await client.ExecuteAsync<UserDto>(request);
        }

        public async Task<ApiResponse> Register(UserDto user)
        {
            BaseRequest request = new BaseRequest();
            request.Method = RestSharp.Method.Post;
            request.Route = $"api/{serviceName}/Register";
            request.Parameter = user;
            return await client.ExecuteAsync(request);
        }
    }
}
