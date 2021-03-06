using System;
using FluentMigrator.Runner;

using MetricsManager.DataBase;
using MetricsManager.DataBase.Interfaces;
using MetricsManager.DataBase.Repositories;
using MetricsManager.Jobs;
using MetricsManager.Jobs.MetricsJobs;
using MetricsManager.Services;
using MetricsManager.Services.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Quartz.Impl;
using Quartz.Spi;
using Quartz;
using System.IO;
using System.Reflection;

namespace MetricsManager
{
    public class Startup
    {
        private const string ConnectionString = "Data Source=metrics.db;Version=3;Pooling=true;Max Pool Size=100;";
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() {Title = "Metrics manager api", Version = "v1"});

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddServices();

            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(ConnectionString)
                    .ScanIn(typeof(Startup).Assembly).For.Migrations())
                .AddLogging(logging => logging.AddFluentMigratorConsole());

            services.AddAutoMapper(typeof(MapperProfile));
            services.AddSingleton(new SQLiteContainer(ConnectionString));

            services
                .AddSingleton<IAgentsRepository, AgentsRepository>()
                .AddSingleton<ICpuMetricsRepository, CpuMetricsRepository>()
                .AddSingleton<IDotnetMetricsRepository, DotnetMetricsRepository>()
                .AddSingleton<IHddMetricsRepository, HddMetricsRepository>()
                .AddSingleton<INetworkMetricsRepository, NetworkMetricsRepository>()
                .AddSingleton<IRamMetricsRepository, RamMetricsRepository>()
                ;

            services.AddHttpClient<IMetricsClient, MetricsClient>()
                .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(4, i => TimeSpan.FromMilliseconds(1000*i)));

            services
                .AddSingleton<IJobFactory, JobFactory>()
                .AddSingleton<ISchedulerFactory, StdSchedulerFactory>()
                .AddHostedService<QuartzHostedService>();
            services
                .AddJob<CpuMetricJob>("0/5 * * * * ?")
                .AddJob<DotnetMetricJob>("0/5 * * * * ?")
                .AddJob<HddMetricJob>("0/5 * * * * ?")
                .AddJob<NetworkMetricJob>("0/5 * * * * ?")
                .AddJob<RamMetricJob>("0/5 * * * * ?")
                ;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMigrationRunner migrationRunner)
        {
            migrationRunner.MigrateUp();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger().UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json","Metrics manager api v1"));
            }

            
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}