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
        Task<ApiResponse<List<TEntity>>> GetAllAsync(string userName);

        Task<ApiResponse<TEntity>> GetFirstOfDefaultAsync(string userName);

        Task<ApiResponse<TEntity>> AddAsync(TEntity entity);

        Task<ApiResponse<TEntity>> UpdateAsync(TEntity entity);

        Task<ApiResponse> DeleteAsync(int id);
    }
}
