
-- 1. Xóa Index trên EmployeeCode
DROP INDEX IX_Employee_EmployeeCode ON Employee;

-- 2. Xóa Index trên DepartmentID
DROP INDEX IX_Employee_DepartmentID ON Employee;

-- 3. Xóa Index trên PositionID
DROP INDEX IX_Employee_PositionID ON Employee;

-- 4. Xóa Composite Index
DROP INDEX IX_Employee_Dept_IsDeleted ON employee;


-- 1. Index cho mã nhân viên (Tăng tốc CheckDuplicate và tìm kiếm theo mã)
-- UNIQUE đảm bảo không có hai nhân viên trùng mã
CREATE UNIQUE INDEX IX_Employee_EmployeeCode ON Employee (EmployeeCode);

-- 2. Index cho phòng ban (Tăng tốc JOIN và Filter theo phòng ban)
CREATE INDEX IX_Employee_DepartmentID ON Employee (DepartmentId);

-- 3. Index cho vị trí (Tối ưu hóa bộ lọc chức vụ)
CREATE INDEX IX_Employee_PositionID ON Employee (PositionId);

-- 4. Composite Index (Index tổ hợp - CỰC KỲ QUAN TRỌNG cho GetPage)
-- Tối ưu cho các truy vấn vừa lọc phòng ban vừa loại bỏ bản ghi đã xóa (IsDeleted = 0)
CREATE INDEX IX_Employee_Dept_IsDeleted ON Employee (DepartmentId, IsDeleted);



