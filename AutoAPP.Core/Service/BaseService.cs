using AutoAPP.Core.Service.Client;
using AutoAPP.Core.Service.Interface;
using Newtonsoft.Json;
using AutoShared;
using AutoShared.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QueryParameter = AutoShared.Parameters.QueryParameter;

namespace AutoAPP.Core.Service
{
    public class BaseService<TEntity>(HttpRestClient client, string serviceName) : IBaseService<TEntity> where TEntity : class
    {
        private readonly HttpRestClient client = client;
        private readonly string serviceName = serviceName;

        public async Task<ApiResponse<TEntity>> AddAsync(TEntity entity)
        {
            BaseRequest request = new BaseRequest();
            request.Method = RestSharp.Method.Post;
            request.Route = $"api/{serviceName}/Add";
            request.Parameter = entity;
            return await client.ExecuteAsync<TEntity>(request);
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            BaseRequest request = new BaseRequest();
            request.Method = RestSharp.Method.Delete;
            request.Route = $"api/{serviceName}/Delete?id={id}";
            return await client.ExecuteAsync(request);
        }

        public async Task<ApiResponse<List<TEntity>>> GetAllAsync(string userName)
        {
            BaseRequest request = new BaseRequest();
            request.Method = RestSharp.Method.Get;
            request.Route = $"api/{serviceName}/GetAll?userName={userName}";
            return await client.ExecuteAsync<List<TEntity>>(request);
        }

        public async Task<ApiResponse<TEntity>> GetFirstOfDefaultAsync(string userName)
        {
            BaseRequest request = new BaseRequest();
            request.Method = RestSharp.Method.Get;
            request.Route = $"api/{serviceName}/Get?userName={userName}";
            return await client.ExecuteAsync<TEntity>(request);
        }

        public async Task<ApiResponse<TEntity>> UpdateAsync(TEntity entity)
        {
            BaseRequest request = new BaseRequest();
            request.Method = RestSharp.Method.Post;
            request.Route = $"api/{serviceName}/Update";
            request.Parameter = entity;
            return await client.ExecuteAsync<TEntity>(request);
        }
    }
}
