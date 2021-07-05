using Dapper;
using SiamCross.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public class AttrDir
    {
        public readonly Dictionary<uint, AttributeItem> ById = new Dictionary<uint, AttributeItem>();
        public readonly Dictionary<string, AttributeItem> ByTitle = new Dictionary<string, AttributeItem>();


        private void AddItemToIndex(AttributeItem item)
        {
            ById[item.Id] = item;
            ByTitle[item.Title] = item;
        }
        public async Task InitAsync()
        {
            ById.Clear();
            ByTitle.Clear();
            IEnumerable<AttributeItem> values;
            using (var tr = BeginTransaction())
                values = await LoadAsync(tr);
            foreach (var v in values)
                AddItemToIndex(v);
        }

        private async Task<IEnumerable<AttributeItem>> LoadAsync(IDbTransaction tr)
        {
            var values = await tr.Connection.QueryAsync<AttributeItem>(select_all);
            return values;
        }
        public async Task<AttributeItem> SaveAsync(string title, AttributeType typeId)
        {
            var item = new AttributeItem()
            {
                Title = title,
                TypeId = (int)typeId
            };
            using (var tr = BeginTransaction())
            {
                await tr.Connection.ExecuteAsync(insert_with_default_id, item);
                tr.Commit();
            }
            using (var tr = BeginTransaction())
            {
                var values = await tr.Connection.QueryAsync<AttributeItem>(select_by_title_and_type, item);
                if (null == values)
                    return null;
                using (var enumerator = values.GetEnumerator())
                {
                    enumerator.MoveNext();
                    if (null == enumerator.Current)
                        return null;
                    item.Id = enumerator.Current.Id;
                    AddItemToIndex(item);
                    return item;
                }
            }
        }
        IDbTransaction BeginTransaction()
        {
            return DbService.Instance.Db.BeginTransaction(IsolationLevel.Serializable);
        }
        private static readonly string insert_with_default_id
            = $"INSERT OR REPLACE INTO Attributes(Title, TypeId) VALUES(@Title, @TypeId)";
        private static readonly string select_all
            = $"SELECT * FROM Attributes";
        private static readonly string select_by_title_and_type
            = $"SELECT * FROM Attributes WHERE Title=@Title AND TypeId=@TypeId";

    }
}
