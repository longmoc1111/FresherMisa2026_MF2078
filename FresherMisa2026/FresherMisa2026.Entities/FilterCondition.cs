using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Entities
{
    public class FilterCondition
    {
        public string? ColumnName { get; set; }
        public string? Operator { get; set; }
        public string? value { get; set; }   
    }
}
