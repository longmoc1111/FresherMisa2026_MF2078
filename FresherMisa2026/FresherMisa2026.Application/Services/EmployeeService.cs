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

        protected override bool ValidateCustom(Employee employee)
        {
            var errorMessage = new List<string>();
            var isValid = true;
            // 1. Lấy thông tin cột Unique từ Attribute đã cấu hình ở Entity
            // (Giả sử bạn đã đặt [ConfigTable(uniqueColumns: "EmployeeCode")])
            var uniqueColumns = _modelType.GetUnique();
            var emailColumn = _modelType.GetEmail();
            var phoneColumn = _modelType.GetPhone();
            var DateOfBirthColumn = _modelType.GetDate();
            if (!string.IsNullOrEmpty(uniqueColumns))
            {
                // 2. Tách lấy tên cột cần check (phòng trường hợp bạn nhập nhiều cột cách nhau bởi dấu phẩy)
                var columnName = uniqueColumns.Split(',')[0].Trim();
                var prop = employee.GetType().GetProperty(columnName);
                if (prop != null)
                {
                    var valueUnique = prop.GetValue(employee);
                    var keyName = _modelType.GetKeyName();
                    var id = (Guid)employee.GetType().GetProperty(keyName).GetValue(employee);
                  
                    // 3. Gọi xuống Repository để kiểm tra trùng lặp
                    // Vì ValidateCustom là hàm đồng bộ, ta dùng .GetAwaiter().GetResult() để đợi kết quả Task
                    var count = _EmployeeRepo.CheckDuplicate(valueUnique, id).GetAwaiter().GetResult();

                 
                    if (count > 0)
                    {
                        // 4. Thiết lập thông báo lỗi vào ServiceResult (kế thừa từ BaseService)
                        errorMessage.Add("Mã nhân viên đã tồn tại!");

                        isValid = false;
                    }
                }
            }
          
            if (emailColumn != null)
            {
                var propEmail = employee.GetType().GetProperty(emailColumn);
                var valueEmail = propEmail.GetValue(employee);
                //kiểm tra đinh dạng email
                if (valueEmail != null && !valueEmail.ToString().IsValidEmail())
                {
                    errorMessage.Add("Email không đúng định dạng!");
                    isValid = false;

                }
            }
            if (phoneColumn != null)
            {
                var propPhone = employee.GetType().GetProperty(phoneColumn);
                var valuePhone = propPhone.GetValue(employee);
                //kiểm tra đinh dạng email
                if (valuePhone != null && !valuePhone.ToString().IsValidPhone())
                {
                    errorMessage.Add("Số Điện thoại không đúng định dạng!");
                    isValid = false;

                }
            }

            if(DateOfBirthColumn != null)
            {
                var propDateOfbirth = employee.GetType().GetProperty(DateOfBirthColumn);
                var valueDateOfBirth = propDateOfbirth.GetValue(employee);
                if(valueDateOfBirth != null && valueDateOfBirth is DateTime date)
                {
                    if (!((DateTime?)date).IsBeforeCurrentDate())
                    {
                        errorMessage.Add("Ngày sinh phải nhỏ hơn ngày hiện tại!");
                        isValid = false;
                    }
                }
            }                                                   
            

            if (!isValid)
            {
                _serviceResult.IsSuccess = false;
                _serviceResult.Code = (int)Entities.Enums.ResponseCode.BadRequest;
                _serviceResult.Data = errorMessage;
                _serviceResult.DevMessage = "Validate custom failed: Email, Phone invalid.";
                return false;
            }


            // Nếu không trùng hoặc không có cấu hình Unique thì trả về true
            return true;
        }
    }
}
