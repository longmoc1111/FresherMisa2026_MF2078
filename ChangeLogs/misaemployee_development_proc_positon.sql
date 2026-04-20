DELIMITER ;;

-- 1. Thủ tục Thêm mới Chức vụ
/*!50003 DROP PROCEDURE IF EXISTS `Proc_InsertPosition` */;
CREATE DEFINER=`root`@`localhost` PROCEDURE `Proc_InsertPosition`(
    IN v_PositionID CHAR(36),
    IN v_PositionCode VARCHAR(20),
    IN v_PositionName VARCHAR(255),
    IN v_CreatedBy VARCHAR(100)
)
BEGIN
    -- Kiểm tra trùng mã (chỉ kiểm tra với bản ghi chưa xóa)
    IF EXISTS (SELECT 1 FROM `position` WHERE PositionCode = v_PositionCode AND IsDeleted = 0) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'PositionCode đã tồn tại';
    ELSE
        INSERT INTO `position` (
            PositionID, PositionCode, PositionName, 
            CreatedDate, CreatedBy, IsDeleted
        )
        VALUES (
            v_PositionID, v_PositionCode, v_PositionName, 
            NOW(), v_CreatedBy, 0
        );
    END IF;
END ;;

-- 2. Thủ tục Cập nhật Chức vụ
/*!50003 DROP PROCEDURE IF EXISTS `Proc_UpdatePosition` */;
CREATE DEFINER=`root`@`localhost` PROCEDURE `Proc_UpdatePosition`(
    IN v_PositionID CHAR(36),
    IN v_PositionCode VARCHAR(20),
    IN v_PositionName VARCHAR(255),
    IN v_ModifiedBy VARCHAR(100)
)
BEGIN
    -- Kiểm tra tồn tại
    IF NOT EXISTS (SELECT 1 FROM `position` WHERE PositionID = v_PositionID) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Chức vụ không tồn tại';
    END IF;

    -- Kiểm tra trùng mã với bản ghi khác
    IF EXISTS (SELECT 1 FROM `position` WHERE PositionCode = v_PositionCode AND PositionID <> v_PositionID AND IsDeleted = 0) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'PositionCode đã tồn tại';
    END IF;

    UPDATE `position`
    SET 
        PositionCode = v_PositionCode,
        PositionName = v_PositionName,
        ModifiedDate = NOW(),
        ModifiedBy = v_ModifiedBy
    WHERE PositionID = v_PositionID;
END ;;

-- 3. Thủ tục Xóa mềm Chức vụ
/*!50003 DROP PROCEDURE IF EXISTS `Proc_DeletePositionById` */;
CREATE DEFINER=`root`@`localhost` PROCEDURE `Proc_DeletePositionById`(
    IN v_PositionID CHAR(36)
)
BEGIN
    UPDATE `position` 
    SET IsDeleted = 1 
    WHERE PositionID = v_PositionID;
END ;;

DELIMITER ;