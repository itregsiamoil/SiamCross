using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SiamCross.Services.RepositoryTables
{
    public class DataItem<T>
    {
        public long MeasureId { get; set; }
        public string Title { get; set; }
        public T Value { get; set; }
    }

    public class DataTable<T>
    {
        private readonly IDbConnection _db;
        private readonly string insert_sql;
        private readonly string select_by_id_sql;
        public DataTable(IDbConnection db, string table)
        {
            _db = db;
            insert_sql = $"INSERT INTO {table}(MeasureId, Title, Value) VALUES(@MeasureId, @Title, @Value)";
            select_by_id_sql = $"SELECT * FROM {table} WHERE MeasureId=@MeasureId";
        }

        public async Task Save(long measureId, Dictionary<string, T> values)
        {
            foreach (var v in values)
            {
                var param = new
                {
                    MeasureId = measureId,
                    Title = v.Key,
                    Value = v.Value,
                };
                await _db.ExecuteAsync(insert_sql, param);
            }
        }
        public async Task<Dictionary<string, T>> Load(long measureId)
        {
            //const string sql = "SELECT * FROM  WHERE MeasureId=@MeasureId";
            var values = await _db.QueryAsync<DataItem<T>>(select_by_id_sql, param: new { MeasureId = measureId });
            var dict = new Dictionary<string, T>();
            foreach (var v in values)
                dict.Add(v.Title, v.Value);
            return dict;
        }
    }

    public class DataInt : DataTable<long> { public DataInt(IDbConnection db) : base(db, "ValInt") { } }
    public class DataFloat : DataTable<double> { public DataFloat(IDbConnection db) : base(db, "ValFloat") { } }
    public class DataString : DataTable<string> { public DataString(IDbConnection db) : base(db, "ValString") { } }
    public class DataBlob : DataTable<string> { public DataBlob(IDbConnection db) : base(db, "ValBlob") { } }
}
