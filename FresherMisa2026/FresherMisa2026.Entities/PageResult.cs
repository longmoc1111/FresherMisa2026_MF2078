using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Entities
{
    /// <summary>
    /// cấu trúc trả về cho API dữ liệu phân trang
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageResult<T>
    {
       
        public int PageIndex { get; set; }  
        public int PageSize { get; set; }
        public long Total { get; set; }
        public IEnumerable<T> Data { get; set; }


    }
    public class PagingRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public List<FilterCondition>? Filters { get; set; }
    }
}
