using System.Reflection;

using ERP.Application.Behaviors;
using ERP.Application.Services;

using FluentValidation;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace ERP.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // MediatR 등록
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // AutoMapper 등록
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // FluentValidation 등록
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Pipeline Behaviors 등록
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            // Application Services 등록
            services.AddScoped<ProjectApplicationService>();
            services.AddScoped<EmployeeApplicationService>();
            services.AddScoped<DashboardService>();

            return services;
        }
    }
}