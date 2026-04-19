using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service dùng chung
    /// </summary>
    /// <typeparam name="TEntity">Loại thực thể</typeparam>
    /// CREATED BY: DVHAI (11/07/2026)
    public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : BaseModel
    {
        #region Declare
        IBaseRepository<TEntity> _baseRepository;
        protected ServiceResponse _serviceResult = null;
        public Type _modelType = null;
        protected string _tableName = string.Empty;
        #endregion

        #region Constructer
        public BaseService(IBaseRepository<TEntity> baseRepository)
        {
            _baseRepository = baseRepository;
            _modelType = typeof(TEntity);
            _tableName = _modelType.GetTableName().ToLowerInvariant();
            _serviceResult = new ServiceResponse()
            {
                IsSuccess = true,
                Code = (int)ResponseCode.Success,
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <returns>Danh sách bản ghi</returns>
        /// CREATED BY: DVHAI 11/07/2026
        public async Task<IEnumerable<TEntity>> GetEntities()
        {
            var entities = await _baseRepository.GetEntities();
            return entities.Cast<TEntity>();
        }

        /// <summary>
        /// Lấy bản ghi theo Id
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Bản ghi duy nhất</returns>
        /// CREATED BY: DVHAI (11/07/2026)
        public async Task<TEntity> GetEntityByID(Guid entityId)
        {
            var entity = await _baseRepository.GetEntityByID(entityId);
            return entity;
        }

        /// <summary>
        /// Xóa bản ghi
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns>Số dòng bị xóa</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        public async Task<bool> DeleteByID(Guid entityId)
        {
            int rowAffects = await _baseRepository.Delete(entityId);
            if(rowAffects > 0)
                AfterDelete();
            return rowAffects > 0;
        }

        /// <summary>
        /// Validate tất cả
        /// </summary>
        /// <param name="entity">Thực thể</param>
        /// <returns>(true-đúng false-sai)</returns>
        /// CREATED BY: DVHAI (07/07/2021)
        private bool Validate(TEntity entity)
        {
            var isValid = true;

            //1. Đọc các property
            var properties = entity.GetType().GetProperties();

            foreach (var property in properties)
            {
                //1.1 Kiểm tra xem  có attribute cần phải validate không
                if (isValid && property.IsDefined(typeof(IRequired), false))
                {
                    //1.1.1 Check bắt buộc nhập
                    isValid = ValidateRequired(entity, property);
                    isValid = ValidateRequired(entity, property);
                }
            }

            //2. Validate tùy chỉnh từng màn hình
            if (isValid)
            {
                isValid = ValidateCustom(entity);
            }

            return isValid;
        }

        /// <summary>
        /// Validate bắt buộc nhập
        /// </summary>
        /// <param name="entity">Thực thể</param>
        /// <param name="propertyInfo">Thuộc tính của thực thể</param>
        /// <returns>(true-đúng false-sai)</returns>
        /// CREATED BY: DVHAI (07/07/2021)
        private bool ValidateRequired(TEntity entity, PropertyInfo propertyInfo)
        {
            bool isValid = true;

            //1. Tên trường
            var propertyName = propertyInfo.Name;

            //2. Giấ trị
            var propertyValue = propertyInfo.GetValue(entity);

            //3. Tên hiển thị
            var propertyDisplayName = _modelType.GetColumnDisplayName(propertyName);

            // 4. Kiểm tra null trước
            if (propertyValue == null)
            {
                isValid = false;
            }       // 6. Nếu là Guid, kiểm tra Guid.Empty
            else if (propertyValue is Guid guid && guid == Guid.Empty)
            {
                isValid = false;
            }
            // 5. Nếu là string, kiểm tra rỗng hoặc toàn dấu cách
            else if (propertyValue is string str && string.IsNullOrWhiteSpace(str))
            {
                isValid = false;
            }
        

            if (!isValid)
            {
                _serviceResult.Code = (int)ResponseCode.BadRequest;
                _serviceResult.DevMessage = "Required field is empty.";
                _serviceResult.Data = $"{propertyDisplayName} không được để trống!";
            }

            return isValid;
        }

        /// <summary>
        /// Validate từng màn hình
        /// </summary>
        /// <param name="entity">Thực thể</param>
        /// CREATED BY: DVHAI (07/07/2021)
        protected virtual bool ValidateCustom(TEntity entity)
        {
            return true;
        }


        /// <summary>
        /// Thêm một thực thể
        /// </summary>
        /// <param name="entity">Thực thể cần thêm</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public async Task<ServiceResponse> Insert(TEntity entity)
        {
            entity.State = ModelSate.Add;

            //1. Validate tất cả các trường nếu được gắn thẻ
            var isValid = Validate(entity);

            //2. Sử lí lỗi tương ứng
            if (isValid)
            {
                _serviceResult.Data = await _baseRepository.Insert(entity);
                _serviceResult.Code = (int)ResponseCode.Success;
            }
            else
            {
                _serviceResult.Code = (int)ResponseCode.BadRequest;
                _serviceResult.DevMessage = "Validate thất bại";
            }

            //3. Trả về kế quả
            return _serviceResult;
        }
         /// <summary>
         ///   phân trang, lọc,tìm kiếm
         /// </summary>
         /// <param name="page"></param>
         /// <param name="pageSize"></param>
         /// <param name="search"></param>
         /// <param name="filters"></param>
         /// <returns></returns>
        public async Task<PageResult<TEntity>> getPage(int page, int pageSize, string? search = "", List<FilterCondition>? filters = null)
        {
            return await _baseRepository.GetPage(page, pageSize, search, filters);
        }

        /// <summary>
        /// Cập nhập thông tin bản ghi 
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public async Task<ServiceResponse> Update(Guid entityId, TEntity entity)
        {
            //1. Trạng thái
            entity.State = ModelSate.Update;

            //2. Validate tất cả các trường nếu được gắn thẻ
            var isValid = Validate(entity);
            if (isValid)
            {
                int rowAffects = await _baseRepository.Update(entityId, entity);
                _serviceResult.Data = rowAffects;
                if (rowAffects > 0)
                {
                    _serviceResult.Code = (int)ResponseCode.Success;
                }
                else
                {
                    _serviceResult.Code = (int)ResponseCode.BadRequest;
                }
            }
            else
            {
                _serviceResult.Code = (int)ResponseCode.Success;
                _serviceResult.DevMessage = "Validate thất bại";
            }
            //3. Trả về kế quả
            return _serviceResult;
        }
        #endregion

        #region Virtual method
        /// <summary>
        /// Xóa thành công
        /// </summary>
        protected virtual void AfterDelete()
        {
        }

      
        #endregion
    }
}
