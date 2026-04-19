using FresherMisa2026.Entities.Department;
using FresherMisa2026.Entities.Employee;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Application.Interfaces.Repositories
{
    public interface IDepartmentRepo : IBaseRepository<Department>
    {
        /// <summary>
        /// lấy thông tin nhan viên theo mã phòng ban
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<IEnumerable<Employee>> GetEmployeeByDepartmentCode(String code);
        /// <summary>
        /// đếm số lượng nhân viên theo mã phòng ban 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<int> GetCountEmployee(string code);
    }
}
