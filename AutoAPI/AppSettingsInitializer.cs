using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;


namespace AutoAPI
{

    public static class AppSettingsInitializer
    {
        // 使用一个属性来存储生成的 JSON 字符串，只生成一次
        private static string DefaultSettingsContentJson { get; } = GenerateDefaultSettingsContent();

        // 私有静态方法，使用 C# 对象结构生成默认配置的 JSON 字符串
        private static string GenerateDefaultSettingsContent()
        {
            // 使用匿名对象来定义配置结构， mirrors the appsettings.json structure
            // 注意：JSON key 中的点号（如 "Microsoft.AspNetCore"）在 C# 匿名对象属性名中不合法，
            // 但 JsonSerializer 可以通过 Dictionary 来正确处理带有此类字符的 key。
            var defaultSettings = new
            {
                ConnectionStrings = new
                {
                    DbConnection = "Data Source=AutoAPI.db" // 你的默认数据库连接字符串
                },
                Kestrel = new
                {
                    Endpoints = new
                    {
                        Http = new
                        {
                            Url = "http://*:5000" // 默认 HTTP 监听地址和端口
                        },
                        Https = new // 默认 HTTPS 监听地址和端口
                        {
                            Url = "https://*:5001" // 注意：HTTPS 需要额外的证书配置才能实际工作
                        }
                    }
                },
                Logging = new // 日志配置
                {
                    LogLevel = new Dictionary<string, string> // 使用 Dictionary 处理带有 '.' 的 key
                {
                    {"Default", "Information"},
                    {"Microsoft.AspNetCore", "Warning"}
                }
                },
                AllowedHosts = "*" // 允许的主机设置
            };

            // 配置 JsonSerializerOptions 来进行格式化输出（带缩进）
            var options = new JsonSerializerOptions
            {
                WriteIndented = true // 让输出的 JSON 字符串带有缩进，更易读
            };

            // 将 C# 对象序列化为 JSON 字符串
            return JsonSerializer.Serialize(defaultSettings, options);
        }

        // 文件检查和创建方法，现在使用生成的 JSON 字符串
        public static void EnsureAppSettingsFileExists(IHostEnvironment env, ILogger logger)
        {
            var appSettingsPath = Path.Combine(env.ContentRootPath, "appsettings.json");

            if (!File.Exists(appSettingsPath))
            {
                try
                {
                    // 使用生成的 JSON 字符串写入文件
                    File.WriteAllText(appSettingsPath, DefaultSettingsContentJson);
                    logger.LogInformation($"appsettings.json 文件不存在，已使用默认内容创建：{appSettingsPath}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"创建默认 appsettings.json 文件时发生错误：{appSettingsPath}");
                    // 可以选择是否在此处重新抛出异常，如果文件无法创建是致命错误的话
                    // throw;
                }
            }
            else
            {
                // 可选：如果文件存在，也可以记录一下
                // logger.LogInformation($"appsettings.json 文件已存在：{appSettingsPath}");
            }
        }
    }
}
