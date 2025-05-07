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
using System.Collections.ObjectModel;

namespace AutoAPI.Service
{
    /// <summary>
    /// 待办事项的实现
    /// </summary>
    public class RecordService(IUnitOfWork work, IMapper mapper) : IRecordService
    {
        private readonly IUnitOfWork Work = work;
        private readonly IMapper Mapper = mapper;

        public async Task<ApiResponse> GetAllAsync(string userName)
        {
            try
            {
                var repository = Work.GetRepository<RecordEntity>();
                if (userName == "Admin")
                {
                    var result = await repository.GetAllAsync(
                        orderBy: source => source.OrderByDescending(t => t.CreateDate)
                        );
                    return new ApiResponse(true, result);
                }
                else
                {
                    var result = await repository.GetAllAsync(
                        predicate: x => x.UserName == userName,
                        orderBy: source => source.OrderByDescending(t => t.CreateDate)
                        );
                    return new ApiResponse(true, result);
                }
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
                var repository = Work.GetRepository<RecordEntity>();
                var result = await repository.GetFirstOrDefaultAsync(predicate: x => x.UserName == userName);
                return new ApiResponse(true, result);
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateAsync(RecordDto model)
        {
            try
            {
                var updateData = Mapper.Map<RecordEntity>(model);
                var repository = Work.GetRepository<RecordEntity>();
                var result = await repository.GetFirstOrDefaultAsync(predicate: x => x.Id.Equals(updateData.Id));

                result.Title = updateData.Title;
                result.Content = updateData.Content;
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

        public async Task<ApiResponse> AddAsync(RecordDto model)
        {
            try
            {
                var addData = Mapper.Map<RecordEntity>(model);
                await Work.GetRepository<RecordEntity>().InsertAsync(addData);
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
                var repository = Work.GetRepository<RecordEntity>();
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

        public async Task<ApiResponse> Summary()
        {
            try
            {
                // 实训记录结果
                var result = await work.GetRepository<RecordEntity>().GetAllAsync(
                    orderBy: source => source.OrderByDescending(t => t.CreateDate)
                    );

                SummaryDto summary = new SummaryDto
                {
                    Sum = result.Count, // 汇总实训次数
                    CompletedCount = result.Where(t => t.Content == "3").Count() // 统计通过次数
                };
                summary.CompletedRatio = (summary.CompletedCount / (double)summary.Sum).ToString("0%"); // 统计通过率
                summary.RecordList = new ObservableCollection<RecordDto>(Mapper.Map<List<RecordDto>>(result));

                return new ApiResponse(true, summary);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, "");
            }
        }
    }
}
