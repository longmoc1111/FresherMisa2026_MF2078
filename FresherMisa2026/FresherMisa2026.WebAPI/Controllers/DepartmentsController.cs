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
         /// <summary>
         ///  l?y danh sách nhân viên theo mã phòng ban
         /// </summary>
         /// <param name="code"></param>
         /// <returns></returns>
        [HttpGet("{code}/employees")]
        public async Task<ServiceResponse> getEmployeeByDepartmentCode(string code)
        {
            var res = await _IDepartmentService.GetEmployeeByDepartmentCode(code);
            return res;
        }
        /// <summary>
        /// l?y s? l??ng nhân viên thông qua mã phòng ban
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("{code}/employee-count")]
        public async Task<ServiceResponse> GetEmployeeCount(string code)
        {
            var response = await _IDepartmentService.GetElementCount(code);
            return response;
        }
    }
}
