using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FresherMisa2026.Entities.Extensions
{
    public static  class ValidatorExtensions
    {
        /// <summary>
        /// kiểm tra định dạng email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
       public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrEmpty(email)) return true;
            var regex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"; ;
            return  Regex.IsMatch(email, regex);
        }
        /// <summary>
        /// kiểm tra định dạng phone
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
       public static bool IsValidPhone(this string phone)
        {
            if(string.IsNullOrEmpty(phone)) return true;
            var regex = @"^(0[3|5|7|8|9])([0-9]{8})$";
            return Regex.IsMatch(phone, regex);
        }
        /// <summary>
        /// kiêm tra ngày sinh phải nhỏ hơn ngày hiện tại
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool IsBeforeCurrentDate(this DateTime? date)
        {
            if (!date.HasValue) return true;
            return date.Value < DateTime.Now;
        }


    }
}
