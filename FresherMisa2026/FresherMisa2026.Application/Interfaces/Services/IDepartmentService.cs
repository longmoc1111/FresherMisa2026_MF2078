using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Department;
using FresherMisa2026.Entities.Employee;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Application.Interfaces.Services
{
    public interface IDepartmentService :IBaseService<Department>
    {
        /// <summary>
        /// lấy danh sách nhân viên theo mã phòng ban
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<ServiceResponse> GetEmployeeByDepartmentCode(string code);
        /// <summary>
        /// lấy số lượng nhân viên theo mã phòng ban
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<ServiceResponse> GetElementCount(string code);
    }
}
