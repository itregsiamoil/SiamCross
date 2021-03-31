using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SiamCross.Services.RepositoryTables
{
    public class FieldItem
    {
        public long Id { get; set; }
        public string Title { get; set; }
    }
    public class FieldDictionary
    {
        private readonly IDbConnection _db;

        const string table = "FieldDictionary";
        private readonly string insert_with_default_id
            = $"INSERT OR REPLACE INTO {table}(Title) VALUES(@Title)";
        private readonly string insert_with_user_id
            = $"INSERT OR REPLACE INTO {table}(Title, Id) VALUES(@Title,@Id)";
        private readonly string select_by_id
            = $"SELECT Id, Title FROM {table} WHERE Id=@Id";
        private readonly string select_all
            = $"SELECT Id, Title FROM {table}";

        private readonly string delete_by_id
            = $"DELETE FROM {table} WHERE Id=@Id";
        public FieldDictionary()
        {
        }
        public FieldDictionary(IDbConnection db)
        {
            _db = db;
        }

        public async Task Delete(IDbTransaction tr, long id)
        {
            await tr.Connection.ExecuteAsync(delete_by_id, new { Id = id });
        }
        public async Task Save(IDbTransaction tr, string title, long id = 0)
        {
            if (0 == id)
                await tr.Connection.ExecuteAsync(insert_with_default_id, new { Title = title });
            else
                await tr.Connection.ExecuteAsync(insert_with_user_id, new { Title = title, Id = id });
        }
        public async Task<List<FieldItem>> Load(IDbTransaction tr, long id)
        {
            var values = await tr.Connection
                .QueryAsync<FieldItem>(select_by_id, new { Id = id });
            return values.AsList();
        }
        public async Task<List<FieldItem>> Load(IDbTransaction tr)
        {
            var values = await tr.Connection
                .QueryAsync<FieldItem>(select_all);
            return values.AsList();
        }

        public async Task Delete(long id)
        {
            if (null == _db)
                return;
            using (var tr = _db.BeginTransaction(IsolationLevel.Serializable))
            {
                await Delete(tr, id);
                tr.Commit();
            }
        }
        public async Task Save(string title, long id = 0)
        {
            if (null == _db)
                return;
            using (var tr = _db.BeginTransaction(IsolationLevel.Serializable))
            {
                await Save(tr, title, id);
                tr.Commit();
            }
        }
        public async Task<List<FieldItem>> Load(long id)
        {
            if (null == _db)
                return new List<FieldItem>();
            using (var tr = _db.BeginTransaction(IsolationLevel.Serializable))
            {
                return await Load(tr, id);
            }
        }
        public async Task<List<FieldItem>> Load()
        {
            if (null == _db)
                return new List<FieldItem>();
            using (var tr = _db.BeginTransaction(IsolationLevel.Serializable))
            {
                return await Load(tr);
            }
        }
    }
}
