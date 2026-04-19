using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Application
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationDI(
            this IServiceCollection services)
        {
            //base
            services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
            //department
            services.AddScoped<IDepartmentService, DepartmentService>();
            //employee
            services.AddScoped<IEmployeeService, EmployeeService>();
            //position
            services.AddScoped<IPositionService, PositionService>();    

            return services;
        }
    }
}
