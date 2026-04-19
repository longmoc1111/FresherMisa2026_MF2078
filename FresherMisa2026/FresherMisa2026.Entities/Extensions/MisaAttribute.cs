using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Entities.Extensions
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IRequired : Attribute
    {
      
        public IRequired() { 
        }
    }
    /// <summary>
    /// att lấy ra cột cần lọc
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class  MISAFilter : Attribute
    {
        string? Name { get; set; }
        public MISAFilter(string name)
        {
            Name = name;    
        }
    }
    /// <summary>
    /// att lấy ra cột cần tìm kiếm
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MISASearch : Attribute
    {
        public string? Name { get; set; }   
        public MISASearch(string name)
        {
            Name = name;
        }
    }
}
