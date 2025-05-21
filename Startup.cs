using System.Collections.Generic;
using System.Linq;
using DotNetCore.CAP;
using FinalConsistencyDemo.Consumers;
using FinalConsistencyDemo.Data;
using FinalConsistencyDemo.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FinalConsistencyDemo
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration config) => _config = config;

        public void ConfigureServices(IServiceCollection services)
        {
            // 中心库 DbContext
            services.AddDbContext<CentralDbContext>(opt =>
                opt.UseSqlServer(_config.GetConnectionString("Central")));

            // 绑定 Shards 字典  
            var shardDict = _config.GetSection("Shards")
                .Get<Dictionary<string, string>>()
                .ToDictionary(
                    kv => int.Parse(kv.Key),
                    kv => kv.Value
                );
            //数据库迁移使用
            //dotnet tool install --global dotnet-ef --version 6.0.36
            //dotnet ef migrations add InitShard --context ShardDbContext
            //dotnet ef database update --context ShardDbContext

            //services.AddDbContext<ShardDbContext>(opt =>
            //    opt.UseSqlServer(shardDict[0]));

            // 分片上下文工厂：配置所有分库连接
            services.AddSingleton(new ShardDbContextFactory(shardDict));
            services.AddScoped<ShardRouter>();

            // CAP 配置（使用中心库存储消息元数据 + RabbitMQ 发布/订阅）
            services.AddCap(x =>
            {
                x.UseEntityFramework<CentralDbContext>();
                x.UseSqlServer(_config.GetConnectionString("Central"));
                x.UseRabbitMQ(options =>
                {
                    options.HostName = _config["CAP:RabbitMQ:HostName"];
                    options.Port = int.Parse(_config["CAP:RabbitMQ:Port"]);
                    options.UserName = _config["CAP:RabbitMQ:UserName"];
                    options.Password = _config["CAP:RabbitMQ:Password"];
                    options.VirtualHost = _config["CAP:RabbitMQ:VirtualHost"];
                });
                x.FailedRetryCount = _config.GetValue<int>("CAP:FailedRetryCount");
            });

       
          
            services.AddControllers();
            // 注册消费者
            services.AddTransient<FinanceConsumer>();
        }

        public void Configure(IApplicationBuilder app)
        {
            HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}