using AutoAPI.Context;
using AutoAPI.Context.Entity;
using AutoAPI.Context.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace AutoAPI.Context.Repository
{

    public class ConfigRepository(AutoContext dbContext) : Repository<ConfigEntity>(dbContext), IRepository<ConfigEntity>
    {
    }
}
