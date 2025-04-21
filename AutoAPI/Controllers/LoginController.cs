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
    /// 账户控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LoginController(ILoginService service) : ControllerBase
    {
        private readonly ILoginService service = service;

        [HttpPost]
        public async Task<ApiResponse> Login([FromBody] UserDto param) => await service.LoginAsync(param.Account, param.PassWord);

        [HttpPost]
        public async Task<ApiResponse> Register([FromBody] UserDto param) => await service.Register(param);

    }
}
