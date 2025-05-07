using AutoAPI.Service.Interface;
using AutoAPI.Service.Response;
using AutoMapper;
using AutoAPI.Context;
using AutoShared.Dtos;
using AutoShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoAPI.Context.UnitOfWork;
using AutoAPI.Context.Entity;
using AutoShared.Parameters;
using ApiResponse = AutoAPI.Service.Response.ApiResponse;

namespace AutoAPI.Service
{
    public class LoginService(IUnitOfWork work, IMapper mapper) : ILoginService
    {
        private readonly IUnitOfWork Work = work;
        private readonly IMapper mapper = mapper;

        public async Task<ApiResponse> LoginAsync(string Account, string Password)
        {
            try
            {
                Password = Password.GetMD5();

                var model = await Work.GetRepository<UserEntity>().GetFirstOrDefaultAsync(
                    predicate: x => x.Account.Equals(Account) && x.PassWord.Equals(Password)
                    );

                if (model == null)
                    return new ApiResponse("账号或密码错误,请重试！");

                return new ApiResponse(true, new UserDto()
                {
                    Account = model.Account,
                    UserName = model.UserName,
                    Id = model.Id
                });
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, "登录失败！");
            }
        }

        public async Task<ApiResponse> Register(UserDto user)
        {
            try
            {
                var model = mapper.Map<UserEntity>(user);
                var repository = Work.GetRepository<UserEntity>();
                var userModel = await repository.GetFirstOrDefaultAsync(predicate: x => x.Account.Equals(model.Account));

                if (userModel != null)
                    return new ApiResponse($"当前账号:{model.Account}已存在,请重新注册！");

                model.CreateDate = DateTime.Now;
                model.PassWord = model.PassWord.GetMD5();
                await repository.InsertAsync(model);

                if (await Work.SaveChangesAsync() > 0)
                    return new ApiResponse(true, model);

                return new ApiResponse("注册失败,请稍后重试！");
            }
            catch (Exception ex)
            {
                return new ApiResponse("注册账号失败！");
            }
        }

        public async Task<ApiResponse> GetAllAsync()
        {
            try
            {
                var repository = Work.GetRepository<UserEntity>();
                var result = await repository.GetAllAsync(
                    orderBy: source => source.OrderByDescending(t => t.CreateDate)
                    );
                return new ApiResponse(true, result.Select(t => t.UserName).ToList());
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }
    }
}
