 using Dapper;
using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Extensions;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    // Base repository
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// Created By: dvhai (09/04/2026)
    public class BaseRepository<TEntity> : IBaseRepository<TEntity>, IDisposable where TEntity : BaseModel
    {
        //Properties
        string _connectionString = string.Empty;
        IConfiguration _configuration;
        protected IDbConnection _dbConnection = null;
        protected string _tableName;
        public Type _modelType = null;


        //Constructor
        public BaseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            _dbConnection = new MySqlConnection(_connectionString);
            _modelType = typeof(TEntity);
            _tableName = _modelType.GetTableName();
        }


        /// <summary>
        /// Dispose connection
        /// </summary>
        /// Created By: dvhai (09/04/2026)
        public void Dispose()
        {
            if (_dbConnection != null && _dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
                _dbConnection.Dispose();
            }
        }

        #region Method Get
        /// <summary>
        /// Lấy danh sách entity
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// Created By: dvhai (09/04/2026)
        public async Task<IEnumerable<BaseModel>> GetEntities()
        {
            return await GetEntitiesUsingCommandTextAsync();
        }

        /// <summary>
        /// Lấy tất cả theo command text
        /// </summary>
        /// <returns></returns>
        /// CREATED BY: DVHAI (11/07/2021)
        private async Task<IEnumerable<TEntity>> GetEntitiesUsingCommandTextAsync()
        {
            var query = new StringBuilder($"select * from {_tableName}");
            int whereCount = 0;

            if (_modelType.GetHasDeletedColumn())
            {
                whereCount++;
                query.Append($" where IsDeleted = FALSE");
            }

            var entities = await _dbConnection.QueryAsync<TEntity>(query.ToString(), commandType: CommandType.Text);

            return entities.ToList();
        }

        // <summary>
        ///  Lấy bản ghi theo id
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Bản ghi thông tin 1 bản ghi</return
        /// CREATED BY: DVHAI (07/07/2021)
        public async Task<TEntity> GetEntityByID(Guid entityId)
        {
            return await GetEntitieByIdUsingCommandTextAsync(entityId.ToString());
        }

        /// <summary>
        /// Lấy bản ghi theo id dùng command text
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<TEntity> GetEntitieByIdUsingCommandTextAsync(string id)
        {
            var query = new StringBuilder($"select * from {_tableName}");
            int whereCount = 0;

            Func<StringBuilder, bool> AppendWhere = (query) => { if (whereCount == 0) query.Append(" where "); return true; };

            var primaryKey = _modelType.GetKeyName();

            if (primaryKey != null)
            {
                AppendWhere(query);
                query.Append($"{primaryKey} = '{id}'");
                whereCount++;
            }

            if (_modelType.GetHasDeletedColumn())
            {
                AppendWhere(query);
                query.Append($"IsDeleted = FALSE");
                whereCount++;
            }

            var entities = await _dbConnection.QueryFirstOrDefaultAsync<TEntity>(query.ToString(), commandType: CommandType.Text);

            return entities;
        }

        /// <summary>
        /// Xóa theo mã
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public async Task<int> Delete(Guid entityId)
        {
            var rowAffects = 0;
            _dbConnection.Open();

            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    //1. Lấy tên của khóa chính
                    var keyName = _modelType.GetKeyName();

                    var dynamicParams = new DynamicParameters();
                    dynamicParams.Add($"@v_{keyName}", entityId);

                    //2. Kết nối tới CSDL:
                    rowAffects = await _dbConnection.ExecuteAsync($"Proc_Delete{_tableName}ById", param: dynamicParams, transaction: transaction, commandType: CommandType.StoredProcedure);

                    transaction.Commit();
                }
                catch { transaction.Rollback(); }
            }

            //3. Trả về số bản ghi bị ảnh hưởng
            return rowAffects;
        }


        /// <summary>
        /// Thêm bản ghi
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public async Task<int> Insert(TEntity entity)
        {
            var rowAffects = 0;
            _dbConnection.Open();
            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    //1.Duyệt các thuộc tính trên bản ghi và tạo parameters
                    var parameters = MappingDbType(entity);

                    //2.Thực hiện thêm bản ghi
                    rowAffects = await _dbConnection.ExecuteAsync($"Proc_Insert{_tableName}", param: parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }

            //3.Trả về số bản ghi thêm mới
            return rowAffects;
        }

        /// <summary>
        /// Cập nhập bản ghi
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public async Task<int> Update(Guid entityId, TEntity entity)
        {
            var rowAffects = 0;
            _dbConnection.Open();
            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    //1. Duyệt các thuộc tính trên customer và tạo parameters
                    var parameters = MappingDbType(entity);

                    //2. Ánh xạ giá trị id
                    var keyName = _modelType.GetKeyName();
                    entity.GetType().GetProperty(keyName).SetValue(entity, entityId);

                    //3. Kết nối tới CSDL:
                    rowAffects = await _dbConnection.ExecuteAsync($"Proc_Update{_tableName}", param: parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
            //4. Trả về dữ liệu
            return rowAffects;
        } 
          /// <summary>
          /// hàm check trùng lặp
          /// </summary>
          /// <param name="value"></param>
          /// <param name="id"></param>
          /// <returns></returns>
        public async Task<int> CheckDuplicate(object value , Guid id)
        {
            var keyName = _modelType.GetKeyName();
            var tableName = _modelType.GetTableName();
            var columnUnique = _modelType.GetUnique();
            var sql = $"select count(1) from {tableName} where {columnUnique} = @Value And {keyName} <> @Id";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Value = value, Id = id });
            return count;
        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="page"></param>
       /// <param name="pageSize"></param>
       /// <param name="search"></param>
       /// <param name="filters"></param>
       /// <returns></returns>
       /// <exception cref="NotImplementedException"></exception>
        public async Task<PageResult<TEntity>> GetPage(int page, int pageSize, string search, List<FilterCondition>? filters = null)
        {
            _dbConnection.Open();
            using (var transaction = _dbConnection.BeginTransaction())
            {
                //lấy ra tableName cần lọc
                var tableName = _modelType.GetTableName();
                var sqlWhere = "";
                //taoj list filter 
                var listFilter = new List<string>();
                //list các cột cần search
                var listSearch = new List<string>();
                //list param
                var param = new DynamicParameters();
                //lấy toàn bộ prop chứa nhãn MISAFilter
                var allowedColumnName = typeof(TEntity).GetProperties()
                    .Where(p => p.GetCustomAttribute<MISAFilter>() != null)
                    .ToDictionary(p => p.Name, p => p.Name);

                //lấy ra toàn bộ prop chứa nhãn MISAsearch
                var allowerColumnSearch = typeof(TEntity).GetProperties()
                   .Where(p => p.IsDefined(typeof(MISASearch), inherit: true))
                   .Select(p => p.Name).ToList();

                //lấy ra toàn bộ props 
                if (filters != null)
                {
                    for (var i = 0; i < filters.Count; i++)
                    {
                        var filter = filters[i];
                        //chuyển đổi dữ liệu đầu vào 
                        object valueFilter = filter.value;
                        if (valueFilter is JsonElement js)
                        {
                            //value kind cho biết dữ liệu đầu vào thuộc kiểu nào
                            valueFilter = js.ValueKind switch
                            {
                                JsonValueKind.String => js.GetString(),
                                JsonValueKind.Number => js.GetDecimal(),
                                JsonValueKind.True or JsonValueKind.False => js.GetBoolean(),
                                _ => js.ToString()
                            };
                        }

                        //kiểm tra colum có tồn tại hay không
                        if (allowedColumnName.TryGetValue(filter.ColumnName, out var realName))
                        {
                            string paramName = $"@filter{i}";

                            var condition = "";
                           if(filter.Operator != "" && !string.IsNullOrWhiteSpace(filter.Operator))
                            {
                                switch (filter.Operator)
                                {
                                    case "StartWith":
                                        condition = $"{filter.ColumnName} like {paramName}";
                                        valueFilter = $"{valueFilter}%";
                                        break;
                                    case "EndWith":
                                        condition = $"{filter.ColumnName} like {paramName}";
                                        valueFilter = $"%{valueFilter}";
                                        break;
                                    case "GreaterThan":
                                        condition = $"{filter.ColumnName} > {paramName}";
                                        valueFilter = $"{valueFilter}";
                                        break;
                                    case "LessThan":
                                        condition = $"{filter.ColumnName} < {paramName}";
                                        valueFilter = $"{valueFilter}";
                                        break;
                                }
                            }
                            else
                            {
                                condition = $"{filter.ColumnName} = {paramName}";
                            }
                                //thêm vào list filter
                                listFilter.Add(condition);
                            //thêm value vào list pram
                            param.Add(paramName, valueFilter);

                        }
                    }

                }
                //xu ly phan search
                foreach (var columnSearch in allowerColumnSearch)
                {
                    listSearch.Add($"{columnSearch} like @Search");
                }
                 
                //list chưa cả 2 điều kiện filter và search
                var allConditions = new List<string>();
                //nếu mãng filter khác rỗng
                if (listFilter.Count > 0) allConditions.AddRange(listFilter);
                //nếu search đầu vào khác rỗng
                if (search != "" && !string.IsNullOrWhiteSpace(search))
                {
                    var searchColumn = string.Join(" or ", listSearch);
                    allConditions.Add($"(" + searchColumn + ")");
                    param.Add("@Search", search);
                }

                //tạo câu truy vấn where
                sqlWhere = allConditions.Count > 0 ? $"where {string.Join(" and ", allConditions)}" : "";

                //tính offset
                int offset = (page - 1) * pageSize;
                //thêm vào param
                param.Add("@Offset", offset);
                param.Add("PageSize", pageSize);
                //tạo câu truy vấn
                var sql = $"select * from {tableName} {sqlWhere} limit @PageSize offset @Offset";
                //lấy tổng ban ghi
                var countSql = $"select count(*) from {tableName} {sqlWhere}";
                //thực thi truy vấn
                var data = await _dbConnection.QueryAsync<TEntity>(sql, param, transaction: transaction);
                var count = await _dbConnection.ExecuteScalarAsync<int>(countSql, param, transaction: transaction);


                return new PageResult<TEntity>
                {
                    PageIndex = page,
                    PageSize = pageSize,
                    Data = data,
                    Total = count,
                };
            }


        }
          /// <summary>
          /// lấy thông tin bản ghi thông qua mã code
          /// </summary>
          /// <param name="code"></param>
          /// <returns></returns>
        public async Task<int> GetEntityByCode(string code)
        {
            //lấy ra tên bảng 
            var tableName = _modelType.GetTableName();
            //lấy tên cột là mã code
            var columnCode = _modelType.GetCodeColumn();
            //tạo câu truy vấn
            var sql = $"select count(*) from {tableName} where {columnCode} = @Code";
            //thực thi câu truy vấn
            var res = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = code });
            return res;
        }                                

        /// <summary>
        /// Ánh xạ các thuộc tính sang kiểu dynamic
        /// </summary>
        /// <param name="entity">Thực thể</param>
        /// <returns>Dan sách các biến động</returns>
        private DynamicParameters MappingDbType(TEntity entity)
        {
            var parameters = new DynamicParameters();
            try
            {
                //1. Duyệt các thuộc tính trên entity và tạo parameters
                var properties = entity.GetType().GetProperties();

                foreach (var property in properties)
                {
                    var propertyName = property.Name;
                    var propertyValue = property.GetValue(entity);
                    var propertyType = property.PropertyType;
                    if (propertyValue == null)
                    {
                        propertyValue = DBNull.Value;
                    }


                    if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                        parameters.Add($"@v_{propertyName}", propertyValue, DbType.String);
                    else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                    {
                        // Chuyển thành DbType.DateTime hoặc DbType.Date tùy nhu cầu của bạn
                        parameters.Add($"@v_{propertyName}", propertyValue, DbType.DateTime);
                    }

                    else
                        parameters.Add($"@v_{propertyName}", propertyValue);
                }
            }
            catch { }
            //2. Trả về danh sách các parameter
            return parameters;
        }

       

        #endregion
    }
}
