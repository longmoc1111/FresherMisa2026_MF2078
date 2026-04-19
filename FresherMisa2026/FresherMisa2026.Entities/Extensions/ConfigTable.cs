using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Entities.Extensions
{
    public class ConfigTable : Attribute
    {
        public bool HasDeletedColumn { get; set; } = false;
        public string UniqueColumns { get; set; } = string.Empty;
        public string CodeColumn {  get; set; } = string.Empty;  

        public string TableName { get; set; } = string.Empty;
         public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;

        public ConfigTable(string tableName = "", bool hasDeletedColumn = false, string uniqueColumns = "", string codeColumn = "", string email = "", string phone = "", string date = "")
        {
            TableName = tableName;

            HasDeletedColumn = hasDeletedColumn;

            CodeColumn = codeColumn;
            
            UniqueColumns = uniqueColumns;

            Email = email;

            Phone = phone;

            Date = date;

        }
    }
}
