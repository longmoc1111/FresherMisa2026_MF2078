using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.Employee;
using FresherMisa2026.Entities.Position;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Infrastructure.Repositories
{
    public class EmployeeRepo : BaseRepository<Employee> , IEmployeeRepo
    {
        public EmployeeRepo(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
