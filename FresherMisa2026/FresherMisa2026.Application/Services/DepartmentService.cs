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
         /// <summary>
         /// lấy tổng nhân viên theo phòng ban 
         /// </summary>
         /// <param name="code"></param>
         /// <returns></returns>
         /// <exception cref="NotImplementedException"></exception>
        public async  Task<ServiceResponse> GetElementCount(string code)
        {
            var checkExistCode = await _DepartmentRepo.GetEntityByCode(code);
            if(checkExistCode == 0)
            {
                // Trường hợp mã phòng ban không tồn tại
                _serviceResult.IsSuccess = false;
                _serviceResult.Code = (int)ResponseCode.BadRequest;
                _serviceResult.Data = 0; // Hoặc null
                _serviceResult.UserMessage = "Mã phòng ban không tồn tại!";
                return _serviceResult;
            }
           
                var count = await _DepartmentRepo.GetCountEmployee(code);
                _serviceResult.IsSuccess = true;
                _serviceResult.Code = (int)ResponseCode.Success;
                _serviceResult.Data = count; // Trả về số lượng (có thể là 0)
                return _serviceResult;
        }
        /// <summary>
        /// lấy toàn bộ nhân viên thuộc mã phòng ban
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> GetEmployeeByDepartmentCode(string code)
        {
            var checkExistCode = await _DepartmentRepo.GetEntityByCode(code);
            if (checkExistCode == 0)
            {
                // Trường hợp mã phòng ban không tồn tại
                _serviceResult.IsSuccess = false;
                _serviceResult.Code = (int)ResponseCode.BadRequest;
                _serviceResult.Data = 0; // Hoặc null
                _serviceResult.UserMessage = "Mã phòng ban không tồn tại!";
                return _serviceResult;
            }
            var res = await _DepartmentRepo.GetEmployeeByDepartmentCode(code);
            _serviceResult.IsSuccess = true;
            _serviceResult.Code = (int)ResponseCode.Success;
            _serviceResult.Data = res; 
            return _serviceResult;
        }
    }
}
