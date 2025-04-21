using AutoShared;
using AutoShared.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QueryParameter = AutoShared.Parameters.QueryParameter;

namespace AutoAPP.Core.Service.Interface
{
    public interface IBaseService<TEntity> where TEntity : class
    {
        Task<ApiResponse<PagedList<TEntity>>> GetAllAsync(QueryParameter parameter);

        Task<ApiResponse<TEntity>> GetFirstOfDefaultAsync(int id);

        Task<ApiResponse<TEntity>> AddAsync(TEntity entity);

        Task<ApiResponse<TEntity>> UpdateAsync(TEntity entity);

        Task<ApiResponse> DeleteAsync(int id);
    }
}
