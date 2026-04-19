using Dapper;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.Department;
using FresherMisa2026.Entities.Employee;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace FresherMisa2026.Infrastructure.Repositories
{                                                   
    public class DepartmentRepo : BaseRepository<Department>, IDepartmentRepo
    {
        public DepartmentRepo(IConfiguration configuration, IMemoryCache cache) : base(configuration, cache)
        {
        }

        /// <summary>
        /// lấy danh sách nhân viên theo mã phòng ban
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public  async Task<IEnumerable<Employee>> GetEmployeeByDepartmentCode(String code)
        {
           using(var conn = GetOpenConnection())
            {
                var sql = $"select e.* from Employee e inner join Department d On e.DepartmentID = d.DepartmentID where d.DepartmentCode = @Code And e.IsDeleted = 0";
                var res = await conn.QueryAsync<Employee>(sql, new { Code = code });
                return res;
            }
        }
        /// <summary>
        /// đếm số lượng nhân vien theo mã phòng ban
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<int> GetCountEmployee(string code)
        {
            using (var conn = GetOpenConnection())
            {
                var sql = $"select count(e.EmployeeID) from Employee e inner join Department d on e.DepartmentID = d.DepartmentID where d.DepartmentCode = @Code";
                var count = await conn.ExecuteScalarAsync<int>(sql, new { Code = code });
                return count;
            }
        }
    }
}
