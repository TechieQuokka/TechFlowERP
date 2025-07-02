using ERP.Domain.Interfaces;
using ERP.Domain.Services;
using ERP.Infrastructure.Caching;
using ERP.Infrastructure.External;
using ERP.Infrastructure.Identity;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Persistence.Repositories;
using ERP.Infrastructure.Services;
using ERP.SharedKernel.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<ErpDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
                {
                    options.EnableRetryOnFailure(maxRetryCount: 3);
                    options.CommandTimeout(30);
                });

                // Enable sensitive data logging in development
                if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            // Repositories
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();

            // Domain Services
            services.AddScoped<ProjectDomainService>();

            // Infrastructure Services
            services.AddScoped<ICurrentTenantService, CurrentTenantService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IEmailService, EmailService>();

            // Caching Configuration
            AddCaching(services, configuration);

            return services;
        }

        private static void AddCaching(IServiceCollection services, IConfiguration configuration)
        {
            var redisConnection = configuration.GetConnectionString("Redis");

            if (!string.IsNullOrEmpty(redisConnection))
            {
                // Production: Use Redis
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "ERP";
                });
            }
            else
            {
                // Development: Use In-Memory Cache
                services.AddDistributedMemoryCache();
            }
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // This method can be used to add repositories individually if needed
            return services;
        }
    }
}