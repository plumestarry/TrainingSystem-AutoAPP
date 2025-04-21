using AutoAPI.Context.Entity;
using AutoAPI.Context;
using AutoAPI.Context.Repository;
using AutoAPI.Context.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using AutoAPI.Service.Interface;
using AutoAPI.Service;
using AutoMapper;
using AutoAPI.Extensions;
using Microsoft.OpenApi.Models;

namespace AutoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 获取一个临时的日志记录器来传递给帮助类
            using var tempLoggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole());
            var earlyLogger = tempLoggerFactory.CreateLogger<Program>();

            // 调用帮助方法
            AppSettingsInitializer.EnsureAppSettingsFileExists(builder.Environment, earlyLogger);

            // Add services to the container.

            // 添加数据库上下文
            // 启用了 Unit of Work 模式，用于管理数据库事务和确保数据一致性
            // 添加自定义仓储
            builder.Services.AddDbContext<AutoContext>(option =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DbConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = "Data Source=AutoAPI.db"; // 硬编码的默认连接字符串
                    earlyLogger.LogWarning("数据库连接字符串 'DbConnection' 未在配置中找到，使用硬编码默认值。");
                }
                option.UseSqlite(connectionString);
            }).AddUnitOfWork<AutoContext>()
            .AddCustomRepository<ConfigEntity, ConfigRepository>()
            .AddCustomRepository<RecordEntity, RecordRepository>()
            .AddCustomRepository<UserEntity, UserRepository>();

            // 服务注册 (Dependency Injection Registration)
            builder.Services.AddTransient<IRecordService, RecordService>();
            builder.Services.AddTransient<IConfigService, ConfigService>();
            builder.Services.AddTransient<ILoginService, LoginService>();

            // AutoMapper 配置和注册
            var automapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfile(new AutoMapperProFile());
            });
            builder.Services.AddSingleton(automapperConfig.CreateMapper());

            // 控制器服务注册
            builder.Services.AddControllers();

            // 添加 Swagger/OpenAPI 支持
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AutoAPI", Version = "v1" });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // 获取 DbContext 实例
                    var dbContext = services.GetRequiredService<AutoContext>();

                    // 确保数据库存在，并应用任何待处理的迁移
                    // 如果数据库不存在，它会在这里被创建，并应用所有已有的迁移文件
                    dbContext.Database.Migrate();

                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("数据库存在并已初始化/更新。");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "确保数据库存在和初始化时发生错误。");
                    // 根据需要处理错误，例如让应用启动失败
                    // throw;
                }
            }

            // Configure the HTTP request pipeline.
            // 配置 HTTP 请求管道
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Swagger 中间件 (无条件启用)
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notebook.API v1"));

            // 路由中间件, 授权中间件
            app.UseRouting();
            app.UseAuthorization();

            // 映射控制器和 Swagger 路由
            app.MapControllers();
            app.MapSwagger();

            app.Run();
        }
    }
}
