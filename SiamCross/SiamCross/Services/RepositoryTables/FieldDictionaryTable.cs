using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SiamCross.Services.RepositoryTables
{
    public class FieldItem
    {
        public uint Id { get; set; }
        public string Title { get; set; }
    }
    public class FieldDictionaryTable
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
        public FieldDictionaryTable()
        {
        }
        public FieldDictionaryTable(IDbConnection db)
        {
            _db = db;
        }

        public async Task DeleteAsync(IDbTransaction tr, long id)
        {
            await tr.Connection.ExecuteAsync(delete_by_id, new { Id = id });
        }
        public async Task<FieldItem> SaveAsync(IDbTransaction tr, string title, uint id = 0)
        {
            var item = new FieldItem()
            {
                Id = id,
                Title = title
            };
            int affectedrow;
            if (0 == id)
                affectedrow = await tr.Connection.ExecuteAsync(insert_with_default_id, item);
            else
                affectedrow = await tr.Connection.ExecuteAsync(insert_with_user_id, item);

            return (0 < affectedrow) ? item : null;
        }
        public async Task<List<FieldItem>> LoadAsync(IDbTransaction tr, long id)
        {
            var values = await tr.Connection
                .QueryAsync<FieldItem>(select_by_id, new { Id = id });
            return values.AsList();
        }
        public async Task<List<FieldItem>> LoadAsync(IDbTransaction tr)
        {
            var values = await tr.Connection
                .QueryAsync<FieldItem>(select_all);
            return values.AsList();
        }

        public async Task DeleteAsync(long id)
        {
            if (null == _db)
                return;
            using (var tr = _db.BeginTransaction(IsolationLevel.Serializable))
            {
                await DeleteAsync(tr, id);
                tr.Commit();
            }
        }
        public async Task<FieldItem> SaveAsync(string title, uint id = 0)
        {
            if (null == _db)
                return null;
            using (var tr = _db.BeginTransaction(IsolationLevel.Serializable))
            {
                var val = await SaveAsync(tr, title, id);
                tr.Commit();
                return val;
            }
        }
        public async Task<List<FieldItem>> LoadAsync(long id)
        {
            if (null == _db)
                return new List<FieldItem>();
            using (var tr = _db.BeginTransaction(IsolationLevel.Serializable))
            {
                return await LoadAsync(tr, id);
            }
        }
        public async Task<List<FieldItem>> LoadAsync()
        {
            if (null == _db)
                return new List<FieldItem>();
            using (var tr = _db.BeginTransaction(IsolationLevel.Serializable))
            {
                return await LoadAsync(tr);
            }
        }



    }
}
