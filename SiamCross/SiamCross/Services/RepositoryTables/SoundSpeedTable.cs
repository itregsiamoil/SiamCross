using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SiamCross.Services.RepositoryTables
{
    public class SoundSpeedItem
    {
        public uint Id { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
    }
    public class SoundSpeedTable
    {
        const string table = "SoundSpeedDictionary";
        private readonly string insert_with_default_id
            = $"INSERT OR REPLACE INTO {table}(Title, Value) VALUES(@Title, @Value)";
        private readonly string insert_with_user_id
            = $"INSERT OR REPLACE INTO {table}(Title, Id, Value) VALUES(@Title,@Id, @Value)";
        private readonly string select_by_id
            = $"SELECT Id, Title, Value FROM {table} WHERE Id=@Id";
        private readonly string select_all
            = $"SELECT Id, Title, Value  FROM {table}";

        private readonly string delete_by_id
            = $"DELETE FROM {table} WHERE Id=@Id";
        public SoundSpeedTable()
        {
        }

        public async Task DeleteAsync(IDbTransaction tr, long id)
        {
            await tr.Connection.ExecuteAsync(delete_by_id, new { Id = id });
        }
        public async Task<SoundSpeedItem> SaveAsync(IDbTransaction tr, string title, uint id, string value)
        {
            var item = new SoundSpeedItem()
            {
                Id = id,
                Title = title,
                Value = value
            };
            int affectedrow;
            if (0 == id)
                affectedrow = await tr.Connection.ExecuteAsync(insert_with_default_id, item);
            else
                affectedrow = await tr.Connection.ExecuteAsync(insert_with_user_id, item);

            return (0 < affectedrow) ? item : null;
        }
        public async Task<IEnumerable<SoundSpeedItem>> LoadAsync(IDbTransaction tr, long id)
        {
            var values = await tr.Connection
                .QueryAsync<SoundSpeedItem>(select_by_id, new { Id = id });
            return values;
        }
        public async Task<IEnumerable<SoundSpeedItem>> LoadAsync(IDbTransaction tr)
        {
            var values = await tr.Connection
                .QueryAsync<SoundSpeedItem>(select_all);
            return values;
        }

    }
}
