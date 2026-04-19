using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.Employee;
using FresherMisa2026.Entities.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using FresherMisa2026.Entities.Extensions;

namespace FresherMisa2026.Application.Services
{
    public class EmployeeService : BaseService<Employee> , IEmployeeService
    {
        IEmployeeRepo _EmployeeRepo;
        public EmployeeService(IEmployeeRepo employeeRepo) : base(employeeRepo)
        {
            _EmployeeRepo = employeeRepo;
        }
    }
}
