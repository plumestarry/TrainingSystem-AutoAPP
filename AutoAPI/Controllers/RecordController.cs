using AutoAPI.Service.Interface;
using AutoAPI.Service.Response;
using AutoShared.Dtos;
using Microsoft.AspNetCore.Mvc;
using AutoAPI.Context;
using AutoShared.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoAPI.Controllers
{
    /// <summary>
    /// 待办事项控制器s
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RecordController(IRecordService service) : ControllerBase
    {
        private readonly IRecordService service = service;

        [HttpGet]
        public async Task<ApiResponse> Get(string userName) => await service.GetSingleAsync(userName);

        [HttpGet]
        public async Task<ApiResponse> GetAll(string userName) => await service.GetAllAsync(userName);

        [HttpGet]
        public async Task<ApiResponse> Summary() => await service.Summary();

        [HttpPost]
        public async Task<ApiResponse> Add([FromBody] RecordDto model) => await service.AddAsync(model);

        [HttpPost]
        public async Task<ApiResponse> Update([FromBody] RecordDto model) => await service.UpdateAsync(model);

        [HttpDelete]
        public async Task<ApiResponse> Delete(int id) => await service.DeleteAsync(id);

    }
}
