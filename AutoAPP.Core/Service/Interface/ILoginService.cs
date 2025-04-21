using AutoShared;
using AutoShared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAPP.Core.Service.Interface
{
    public interface ILoginService
    {
        Task<ApiResponse<UserDto>> Login(UserDto user);

        Task<ApiResponse> Register(UserDto user);
    }
}
