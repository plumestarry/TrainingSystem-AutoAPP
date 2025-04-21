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

            // ��ȡһ����ʱ����־��¼�������ݸ�������
            using var tempLoggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole());
            var earlyLogger = tempLoggerFactory.CreateLogger<Program>();

            // ���ð�������
            AppSettingsInitializer.EnsureAppSettingsFileExists(builder.Environment, earlyLogger);

            // Add services to the container.

            // ������ݿ�������
            // ������ Unit of Work ģʽ�����ڹ������ݿ������ȷ������һ����
            // ����Զ���ִ�
            builder.Services.AddDbContext<AutoContext>(option =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DbConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = "Data Source=AutoAPI.db"; // Ӳ�����Ĭ�������ַ���
                    earlyLogger.LogWarning("���ݿ������ַ��� 'DbConnection' δ���������ҵ���ʹ��Ӳ����Ĭ��ֵ��");
                }
                option.UseSqlite(connectionString);
            }).AddUnitOfWork<AutoContext>()
            .AddCustomRepository<ConfigEntity, ConfigRepository>()
            .AddCustomRepository<RecordEntity, RecordRepository>()
            .AddCustomRepository<UserEntity, UserRepository>();

            // ����ע�� (Dependency Injection Registration)
            builder.Services.AddTransient<IRecordService, RecordService>();
            builder.Services.AddTransient<IConfigService, ConfigService>();
            builder.Services.AddTransient<ILoginService, LoginService>();

            // AutoMapper ���ú�ע��
            var automapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfile(new AutoMapperProFile());
            });
            builder.Services.AddSingleton(automapperConfig.CreateMapper());

            // ����������ע��
            builder.Services.AddControllers();

            // ��� Swagger/OpenAPI ֧��
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
                    // ��ȡ DbContext ʵ��
                    var dbContext = services.GetRequiredService<AutoContext>();

                    // ȷ�����ݿ���ڣ���Ӧ���κδ������Ǩ��
                    // ������ݿⲻ���ڣ����������ﱻ��������Ӧ���������е�Ǩ���ļ�
                    dbContext.Database.Migrate();

                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("���ݿ���ڲ��ѳ�ʼ��/���¡�");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "ȷ�����ݿ���ںͳ�ʼ��ʱ��������");
                    // ������Ҫ�������������Ӧ������ʧ��
                    // throw;
                }
            }

            // Configure the HTTP request pipeline.
            // ���� HTTP ����ܵ�
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Swagger �м�� (����������)
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notebook.API v1"));

            // ·���м��, ��Ȩ�м��
            app.UseRouting();
            app.UseAuthorization();

            // ӳ��������� Swagger ·��
            app.MapControllers();
            app.MapSwagger();

            app.Run();
        }
    }
}
