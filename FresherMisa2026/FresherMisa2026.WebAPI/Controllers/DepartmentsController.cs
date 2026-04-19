using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Application.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Department;
using Microsoft.AspNetCore.Mvc;

namespace FresherMisa2026.WebAPI.Controllers
{
    [ApiController]
    public class DepartmentsController : BaseController<Department>
    {
        IDepartmentService _IDepartmentService;
        public DepartmentsController(IDepartmentService departmentService) : base(departmentService)
        {
            _IDepartmentService = departmentService;
        }
    }
}
