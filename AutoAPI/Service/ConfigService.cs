using AutoAPI.Service.Interface;
using AutoAPI.Service.Response;
using AutoMapper;
using AutoAPI.Context;
using AutoShared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoAPI.Context.UnitOfWork;
using AutoAPI.Context.Entity;
using AutoShared.Parameters;

namespace AutoAPI.Service
{
    /// <summary>
    /// 备忘录的实现
    /// </summary>
    public class ConfigService(IUnitOfWork work, IMapper mapper) : IConfigService
    {
        private readonly IUnitOfWork Work = work;
        private readonly IMapper Mapper = mapper;

        public async Task<ApiResponse> GetAllAsync(string userName)
        {
            try
            {
                var repository = Work.GetRepository<ConfigEntity>();
                var result = await repository.GetAllAsync(
                    predicate: x => x.UserName == userName,
                    orderBy: source => source.OrderByDescending(t => t.CreateDate)
                    );
                return new ApiResponse(true, result);
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> GetSingleAsync(string userName)
        {
            try
            {
                var repository = Work.GetRepository<ConfigEntity>();
                var result = await repository.GetFirstOrDefaultAsync(predicate: x => x.UserName == userName);
                return new ApiResponse(true, result);
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateAsync(ConfigDto model)
        {
            try
            {
                var updateData = Mapper.Map<ConfigEntity>(model);
                var repository = Work.GetRepository<ConfigEntity>();
                var result = await repository.GetFirstOrDefaultAsync(predicate: x => x.UserName.Equals(updateData.UserName));

                result.IPAddress = updateData.IPAddress;
                result.Port = updateData.Port;
                result.SlaveID = updateData.SlaveID;
                result.UpdateDate = DateTime.Now;

                repository.Update(result);

                if (await Work.SaveChangesAsync() > 0)
                    return new ApiResponse(true, result);

                return new ApiResponse("更新数据异常！");
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> AddAsync(ConfigDto model)
        {
            try
            {
                var addData = Mapper.Map<ConfigEntity>(model);
                await Work.GetRepository<ConfigEntity>().InsertAsync(addData);
                if (await Work.SaveChangesAsync() > 0)
                    return new ApiResponse(true, addData);
                return new ApiResponse("添加数据失败");
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            try
            {
                var repository = Work.GetRepository<ConfigEntity>();
                var result = await repository.GetFirstOrDefaultAsync(predicate: x => x.Id.Equals(id));
                repository.Delete(result);
                if (await Work.SaveChangesAsync() > 0)
                    return new ApiResponse(true, "");
                return new ApiResponse("删除数据失败");
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }
    }
}
