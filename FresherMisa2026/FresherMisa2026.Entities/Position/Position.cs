using FresherMisa2026.Entities.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FresherMisa2026.Entities.Position
{
    /// <summary>
    /// Thực thể Chức vụ
    /// </summary>
    /// CREATED BY: DVHAI (13/04/2026)
    [ConfigTable("position", false, "PositionCode")]
    public class Position : BaseModel
    {
        /// <summary>
        /// Khóa chính chức vụ
        /// </summary>
        [Key]
        public Guid PositionID { get; set; }

        /// <summary>
        /// Mã chức vụ
        /// </summary>
        public string PositionCode { get; set; }

        /// <summary>
        /// Tên chức vụ
        /// </summary>
        public string PositionName { get; set; }
    }
}
