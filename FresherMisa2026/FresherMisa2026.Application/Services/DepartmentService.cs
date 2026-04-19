using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Department;
using FresherMisa2026.Entities.Employee;
using FresherMisa2026.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Application.Services
{
    public class DepartmentService : BaseService<Department>, IDepartmentService
    {
        IDepartmentRepo _DepartmentRepo;
        public DepartmentService(IDepartmentRepo departmentRepo) : base(departmentRepo)
        {
            _DepartmentRepo = departmentRepo;
        }
    }
}
