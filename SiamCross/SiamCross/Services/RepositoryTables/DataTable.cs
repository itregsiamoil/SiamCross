using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Dapper;

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
        public DataTable(IDbConnection db)
        {
            _db = db;
        }

        public async Task Save(long measureId, Dictionary<string, T> values)
        {
            const string sql = "INSERT INTO ValFloat(MeasureId, Title, Value) VALUES(@MeasureId, @Title, @Value)";
            foreach (var v in values)
            {
                var param = new
                {
                    MeasureId = measureId,
                    Title = v.Key,
                    Value = v.Value,
                };
                await _db.ExecuteAsync(sql, param);
            }
        }

    }

    public class DataInt: DataTable<long> { public DataInt(IDbConnection db) : base(db) { } }
    public class DataFloat : DataTable<double> { public DataFloat(IDbConnection db) : base(db) { } }
    public class DataString : DataTable<string> { public DataString(IDbConnection db) : base(db) { } }
    public class DataBlob : DataTable<byte[]> { public DataBlob(IDbConnection db) : base(db) { } }
}
