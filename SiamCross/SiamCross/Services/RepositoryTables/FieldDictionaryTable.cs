﻿using Dapper;
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
        public async Task<IEnumerable<FieldItem>> LoadAsync(IDbTransaction tr, long id)
        {
            var values = await tr.Connection
                .QueryAsync<FieldItem>(select_by_id, new { Id = id });
            return values;
        }
        public async Task<IEnumerable<FieldItem>> LoadAsync(IDbTransaction tr)
        {
            var values = await tr.Connection
                .QueryAsync<FieldItem>(select_all);
            return values;
        }

    }
}
