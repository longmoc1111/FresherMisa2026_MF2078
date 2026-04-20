 using Dapper;
using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static Dapper.SqlMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    // Base repository
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// Created By: dvhai (09/04/2026)
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseModel
    {
        //Properties
        string _connectionString = string.Empty;
        IConfiguration _configuration;
        protected string _tableName;
        public Type _modelType = null;
        //thêm memory cache
        IMemoryCache _cache;
        //key dùng quản lý cache cho từng loại entity
        string _cacheKeyAll;


        //Constructor
        public BaseRepository(IConfiguration configuration, IMemoryCache cache)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            _modelType = typeof(TEntity);
            _tableName = _modelType.GetTableName();
            _cache = cache;
            _cacheKeyAll = $"{_modelType.Name}_All";
        }


        /// <summary>
        /// Tạo và mở connection - tối ưu connection pooling
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetOpenConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            if (conn.State != ConnectionState.Open) conn.Open();
            return conn;
        } 


        #region Method Get
        /// <summary>
        /// Lấy danh sách entity
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// Created By: dvhai (09/04/2026)
        public async Task<IEnumerable<TEntity>> GetEntities()
        {
            //kiêm tra trong cache trước
            if(!_cache.TryGetValue(_cacheKeyAll, out IEnumerable<TEntity> entities))
            {
                using(var conn = GetOpenConnection())
                {
                    entities = await GetEntitiesUsingCommandTextAsync(conn);
                    //lưu vào cache trong 5 phút
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                    _cache.Set(_cacheKeyAll, entities, cacheEntryOptions);
                }
            }
            return entities;
        }

        /// <summary>
        /// Lấy tất cả theo command text
        /// </summary>
        /// <returns></returns>
        /// CREATED BY: DVHAI (11/07/2021)
        private async Task<IEnumerable<TEntity>> GetEntitiesUsingCommandTextAsync(IDbConnection conn)
        {
            var query = new StringBuilder($"select * from {_tableName}");
            int whereCount = 0;

            if (_modelType.GetHasDeletedColumn())
            {
                whereCount++;
                query.Append($" where IsDeleted = FALSE");
            }

            var entities = await conn.QueryAsync<TEntity>(query.ToString(), commandType: CommandType.Text);

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
            string cacheKey = $"{_modelType.Name}_{entityId}";
            if (!_cache.TryGetValue(cacheKey, out TEntity entity))
            {
                using (var conn = GetOpenConnection())
                {
                    entity = await GetEntitieByIdUsingCommandTextAsync(conn, entityId.ToString());
                    if (entity != null)
                    {
                        _cache.Set(cacheKey, entity, TimeSpan.FromMinutes(5));
                    }
                }
                  
            }
            return entity;
        }

        /// <summary>
        /// Lấy bản ghi theo id dùng command text
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<TEntity> GetEntitieByIdUsingCommandTextAsync(IDbConnection conn, string id)
        {
            var query = new StringBuilder($"SELECT * FROM {_tableName}");
            var primaryKey = _modelType.GetKeyName();
            query.Append($" WHERE {primaryKey} = @Id");

            if (_modelType.GetHasDeletedColumn())
            {
                query.Append($" AND IsDeleted = FALSE");
            }

            return await conn.QueryFirstOrDefaultAsync<TEntity>(query.ToString(), new { Id = id });
        }

        /// <summary>
        /// Xóa theo mã
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public async Task<int> Delete(Guid entityId)
        {
            using(var conn = GetOpenConnection())
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    //1. Lấy tên của khóa chính
                    var keyName = _modelType.GetKeyName();

                    var dynamicParams = new DynamicParameters();
                    dynamicParams.Add($"@v_{keyName}", entityId);

                    //2. Kết nối tới CSDL:
                    var rowAffects = await conn.ExecuteAsync($"Proc_Delete{_tableName}ById", param: dynamicParams, transaction: transaction, commandType: CommandType.StoredProcedure);
                    //xoa cache sau khi xoas xong
                    ClearCache(entityId);
                    transaction.Commit();
                    return rowAffects;
                }
                catch { transaction.Rollback(); throw; }
            }
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
            using (var conn = GetOpenConnection()) 
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    //1.Duyệt các thuộc tính trên bản ghi và tạo parameters
                    var parameters = MappingDbType(entity);

                    //2.Thực hiện thêm bản ghi
                    rowAffects = await conn.ExecuteAsync($"Proc_Insert{_tableName}", param: parameters, transaction: transaction, commandType: CommandType.StoredProcedure);
                    //clear cache sau khi thêm mới
                    ClearCache();
                    transaction.Commit();
                }
                catch(MySqlException ex)
                {
                    transaction.Rollback();
                    if(ex.Number == 1062 || ex.Number == 1644 || ex.Number == 1213)
                    {
                        throw new Exception(ex.Message);
                    }
                    throw;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
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
            using(var conn = GetOpenConnection())
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    //1. Duyệt các thuộc tính trên customer và tạo parameters
                    var parameters = MappingDbType(entity);

                    //2. Ánh xạ giá trị id
                    var keyName = _modelType.GetKeyName();
                    entity.GetType().GetProperty(keyName).SetValue(entity, entityId);

                    //3. Kết nối tới CSDL:
                     var rowAffects = await conn.ExecuteAsync($"Proc_Update{_tableName}", param: parameters, transaction: transaction, commandType: CommandType.StoredProcedure);
                    transaction.Commit();
                    //4.xóa cache sau khi truy vấn xong
                    ClearCache(entityId);
                    //5. Trả về dữ liệu
                    return rowAffects;
                }
                catch
                {
                    transaction.Rollback();
                    throw ;
                }
            }
      
        } 
          /// <summary>
          /// hàm check trùng lặp
          /// </summary>
          /// <param name="value"></param>
          /// <param name="id"></param>
          /// <returns></returns>
        public async Task<int> CheckDuplicate(object value , Guid id)
        {  
            using(var conn = GetOpenConnection())
            {
                var keyName = _modelType.GetKeyName();
                var tableName = _modelType.GetTableName();
                var columnUnique = _modelType.GetUnique();
                var sql = $"select count(1) from {tableName} where {columnUnique} = @Value And {keyName} <> @Id";
                var count = await conn.ExecuteScalarAsync<int>(sql, new { Value = value, Id = id });
                return count;
            }
           
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
           using(var conn = GetOpenConnection())
            using (var transaction = conn.BeginTransaction())
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
                var data = await conn.QueryAsync<TEntity>(sql, param, transaction: transaction);
                var count = await conn.ExecuteScalarAsync<int>(countSql, param, transaction: transaction);

                // 2. Dừng đồng hồ ngay sau khi lấy xong dữ liệu
                var watch = System.Diagnostics.Stopwatch.StartNew();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

                // In ra console để xem
                Console.WriteLine($"Thời gian truy vấn SQL: {elapsedMs}ms"); 

                transaction.Commit();

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
           using(var conn = GetOpenConnection())
            {
                //lấy ra tên bảng 
                var tableName = _modelType.GetTableName();
                //lấy tên cột là mã code
                var columnCode = _modelType.GetCodeColumn();
                //tạo câu truy vấn
                var sql = $"select count(*) from {tableName} where {columnCode} = @Code";
                //thực thi câu truy vấn
                var res = await conn.ExecuteScalarAsync<int>(sql, new { Code = code });
                return res;
            }
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

        protected void ClearCache(Guid? id = null)
        {
            _cache.Remove(_cacheKeyAll);
            if (id.HasValue)
            {
                _cache.Remove($"{_modelType.Name}_{id.Value}");
            }
        }

        #endregion
    }
}
