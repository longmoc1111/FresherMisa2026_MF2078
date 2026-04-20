DELIMITER $$
-- Drop procedure `Proc_DeleteEmployeeById`
--
DROP PROCEDURE IF EXISTS Proc_DeleteEmployeeById;

--
-- Drop procedure `Proc_InsertEmployee`
--
DROP PROCEDURE IF EXISTS Proc_InsertEmployee;

--
-- Drop procedure `Proc_UpdateEmployee`
--
DROP PROCEDURE IF EXISTS Proc_UpdateEmployee;
--
-- Create procedure `Proc_UpdateEmployee`
--
CREATE DEFINER = 'root'@'localhost'
PROCEDURE Proc_UpdateEmployee (IN v_EmployeeID char(36),
IN v_EmployeeCode varchar(20),
IN v_EmployeeName varchar(100),
IN v_Gender int,
IN v_DateOfBirth date,
IN v_PhoneNumber varchar(50),
IN v_Email varchar(100),
IN v_Address varchar(255),
IN v_DepartmentID char(36),
IN v_PositionID char(36),
IN v_Salary decimal(18, 4),
IN v_HireDate DATE,
IN v_CreatedDate datetime)
BEGIN
  -- Check exists
  IF NOT EXISTS (SELECT
        1
      FROM employee
      WHERE EmployeeID = v_EmployeeID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Employee không tồn tại';
  END IF;

  -- Check duplicate code (except itself)
  IF EXISTS (SELECT
        1
      FROM employee
      WHERE EmployeeCode = v_EmployeeCode
      AND EmployeeID <> v_EmployeeID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'EmployeeCode đã tồn tại';
  END IF;

  -- Update
  UPDATE employee
  SET EmployeeCode = v_EmployeeCode,
      EmployeeName = v_EmployeeName,
      Gender = v_Gender,
      DateOfBirth = v_DateOfBirth,
      PhoneNumber = v_PhoneNumber,
      Email = v_Email,
      Address = v_Address,
      DepartmentID = v_DepartmentID,
      PositionID = v_PositionID,
      Salary = v_Salary,
      HireDate = v_HireDate,
      CreatedDate = v_CreatedDate
  WHERE EmployeeID = v_EmployeeID;
END
$$

--
-- Create procedure `Proc_InsertEmployee`
--
CREATE DEFINER = 'root'@'localhost'
PROCEDURE Proc_InsertEmployee (IN v_EmployeeID char(36),
IN v_EmployeeCode varchar(20),
IN v_EmployeeName varchar(100),
IN v_Gender int,
IN v_DateOfBirth date,
IN v_PhoneNumber varchar(50),
IN v_Email varchar(100),
IN v_Address varchar(255),
IN v_DepartmentID char(36),
IN v_PositionID char(36),
IN v_Salary decimal(18, 4),
IN v_HireDate DATE,
IN v_CreatedDate datetime)
BEGIN
  -- Check duplicate code
  IF EXISTS (SELECT
        1
      FROM employee
      WHERE EmployeeCode = v_EmployeeCode) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'EmployeeCode đã tồn tại';
  ELSE
    INSERT INTO employee (EmployeeID,
    EmployeeCode,
    EmployeeName,
    Gender,
    DateOfBirth,
    PhoneNumber,
    Email,
    Address,
    DepartmentID,
    PositionID,
    Salary,
    HireDate,
    CreatedDate)
      VALUES (v_EmployeeID, v_EmployeeCode, v_EmployeeName, v_Gender, v_DateOfBirth, v_PhoneNumber, v_Email, v_Address, v_DepartmentID, v_PositionID, v_Salary,v_HireDate, v_CreatedDate);
  END IF;
END
$$

--
-- Create procedure `Proc_DeleteEmployeeById`
--
CREATE DEFINER = 'root'@'localhost'
PROCEDURE Proc_DeleteEmployeeById (IN v_EmployeeID char(36))
BEGIN
  DELETE
    FROM Employee
  WHERE EmployeeID = v_EmployeeID;
END
$$

DELIMITER ;Proc_InsertEmployeeProc_InsertEmployee