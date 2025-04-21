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

        public async Task<ApiResponse> GetAllAsync(QueryParameter parameter)
        {
            try
            {
                var repository = Work.GetRepository<ConfigEntity>();
                var result = await repository.GetPagedListAsync(
                   predicate: x => string.IsNullOrWhiteSpace(parameter.Search) || x.Title.Contains(parameter.Search),
                   pageIndex: parameter.PageIndex,
                   pageSize: parameter.PageSize,
                   orderBy: source => source.OrderByDescending(t => t.CreateDate)
                   );
                return new ApiResponse(true, result);
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> GetSingleAsync(int id)
        {
            try
            {
                var repository = Work.GetRepository<ConfigEntity>();
                var result = await repository.GetFirstOrDefaultAsync(predicate: x => x.Id.Equals(id));
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
                var result = await repository.GetFirstOrDefaultAsync(predicate: x => x.Id.Equals(updateData.Id));

                result.Title = updateData.Title;
                result.IPAddress = updateData.IPAddress;
                result.IODefinitions = updateData.IODefinitions;
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
