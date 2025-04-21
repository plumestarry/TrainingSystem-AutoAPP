using AutoAPI.Context.Entity;
using AutoAPI.Context.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace AutoAPI.Context.Repository
{
    public class RecordRepository(AutoContext dbContext) : Repository<RecordEntity>(dbContext), IRepository<RecordEntity>
    {
    }
}
