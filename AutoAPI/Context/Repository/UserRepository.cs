using AutoAPI.Context;
using AutoAPI.Context.Entity;
using AutoAPI.Context.UnitOfWork;

namespace AutoAPI.Context.Repository
{
    public class UserRepository(AutoContext dbContext) : Repository<UserEntity>(dbContext), IRepository<UserEntity>
    {
    }
}
