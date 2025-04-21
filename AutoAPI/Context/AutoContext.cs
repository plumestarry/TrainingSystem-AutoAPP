using AutoAPI.Context.Entity;
using Microsoft.EntityFrameworkCore;

namespace AutoAPI.Context
{
    public class AutoContext(DbContextOptions<AutoContext> options) : DbContext(options)
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<ConfigEntity> UserConfigs { get; set; }
        public DbSet<RecordEntity> UserRecords { get; set; }
    }
}
