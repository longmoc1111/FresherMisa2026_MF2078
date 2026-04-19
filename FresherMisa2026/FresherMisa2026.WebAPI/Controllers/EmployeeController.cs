using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.Department;
using FresherMisa2026.Entities.Employee;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FresherMisa2026.WebAPI.Controllers
{
    [ApiController]
    public class EmployeeController : BaseController<Employee>
    {
        IEmployeeService _EmployeeService;
        public EmployeeController(IEmployeeService employeeService) : base(employeeService)
        {
            _EmployeeService = employeeService;
        }
    }
}
