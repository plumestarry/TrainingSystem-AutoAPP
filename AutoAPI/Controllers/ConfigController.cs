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
    /// 备忘录控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ConfigController(IConfigService service) : ControllerBase
    {
        private readonly IConfigService service = service;

        [HttpGet]
        public async Task<ApiResponse> Get(int id) => await service.GetSingleAsync(id);

        [HttpGet]
        public async Task<ApiResponse> GetAll([FromQuery] QueryParameter param) => await service.GetAllAsync(param);

        [HttpPost]
        public async Task<ApiResponse> Add([FromBody] ConfigDto model) => await service.AddAsync(model);

        [HttpPost]
        public async Task<ApiResponse> Update([FromBody] ConfigDto model) => await service.UpdateAsync(model);

        [HttpDelete]
        public async Task<ApiResponse> Delete(int id) => await service.DeleteAsync(id);
    }
}
