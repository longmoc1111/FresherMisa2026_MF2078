using FresherMisa2026.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Application.Interfaces
{
    public interface IBaseRepository<TEntity>
    {
        // <summary>
        ///  Lấy danh sách thực thể
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Bản ghi thông tin 1 bản ghi</return
        /// CREATED BY: DVHAI (07/07/2026)
        public Task<IEnumerable<TEntity>> GetEntities();

        // <summary>
        ///  Lấy bản ghi theo id
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Bản ghi thông tin 1 bản ghi</return
        /// CREATED BY: DVHAI (07/07/2026)
        Task<TEntity> GetEntityByID(Guid entityId);

        /// <summary>
        /// Xóa bản ghi
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Số bản ghi bị xóa</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        Task<int> Delete(Guid entityId);


        /// <summary>
        /// Thêm bản ghi
        /// </summary>
        /// <param name="enitity">Thông tin bản ghi</param>
        /// <returns>Số bản ghi</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        Task<int> Insert(TEntity enitity);

        /// <summary>
        /// Cập nhập thông tin bản ghi
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// CREATED BY: DVHAI (07/07/2026)

        Task<int> Update(Guid entityId, TEntity entity);
        /// <summary>
        /// hàm kiểm tra trùng
        /// </summary>
        /// <param name="value"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<int> CheckDuplicate(object value, Guid id);

        /// <summary>
        /// lọc, tìm kiếm, phân trang theo danh sách nhân viên
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="search"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        Task<PageResult<TEntity>> GetPage(int  page, int pageSize, string search, List<FilterCondition>? filters = null );
         /// <summary>
         /// lấy thông tin ban ghi thông qua mã code
         /// </summary>
         /// <param name="code"></param>
         /// <returns></returns>
        Task<int> GetEntityByCode(string code);

    }
}
