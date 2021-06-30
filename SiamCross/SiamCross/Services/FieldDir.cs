using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Services
{
    public class FieldItem
    {
        public uint Id { get; set; }
        public string Title { get; set; }
    }
    public class FieldDir
    {
        public readonly Dictionary<string, FieldItem> DictByTitle = new Dictionary<string, FieldItem>();
        public readonly Dictionary<uint, FieldItem> DictById = new Dictionary<uint, FieldItem>();
        public readonly ObservableRangeCollection<FieldItem> FieldList = new ObservableRangeCollection<FieldItem>();

        public async Task InitAsync()
        {
            DictByTitle.Clear();
            DictById.Clear();
            FieldList.Clear();

            using (var tr = BeginTransaction())
            {
                var values = await tr.Connection.QueryAsync<FieldItem>(select_all);
                foreach (var item in values)
                {
                    DictByTitle.Add(item.Title, item);
                    DictById.Add(item.Id, item);
                    FieldList.Add(item);
                }
            }
        }
        public async Task AddAsync(string title, uint id)
        {
            await DeleteAsync(id);
            await DeleteAsync(title);
            var item = new FieldItem()
            {
                Id = id,
                Title = title,
            };
            using (var tr = BeginTransaction())
            {

                int affectedrow = await tr.Connection.ExecuteAsync(insert_with_user_id, item);
                tr.Commit();
                if (0 < affectedrow)
                {
                    DictByTitle.Add(item.Title, item);
                    DictById.Add(item.Id, item);
                    FieldList.Add(item);
                }
            }
        }
        public async Task<FieldItem> DeleteAsync(string title)
        {
            if (!DictByTitle.TryGetValue(title, out FieldItem val))
                return null;
            return await DeleteAsync(val.Id);
        }
        public async Task<FieldItem> DeleteAsync(uint id)
        {
            if (!DictById.TryGetValue(id, out FieldItem val))
                return null;
            using (var tr = BeginTransaction())
            {
                await tr.Connection
                    .ExecuteAsync(delete_by_id, new { Id = id });
                tr.Commit();
            }
            DictByTitle.Remove(val.Title);
            DictById.Remove(id);
            FieldList.Remove(val);
            return val;
        }
        public async Task<List<FieldItem>> DeleteAsync(IEnumerable<uint> arr)
        {
            var list = new List<FieldItem>();
            using (var tr = BeginTransaction())
            {
                foreach (var id in arr)
                {
                    if (DictById.TryGetValue(id, out FieldItem val))
                    {
                        await tr.Connection
                            .ExecuteAsync(delete_by_id, new { Id = id });
                        list.Add(val);
                    }
                }
                tr.Commit();
            }
            foreach (var deletedModel in list)
            {
                DictByTitle.Remove(deletedModel.Title);
                DictById.Remove(deletedModel.Id);
            }
            FieldList.RemoveRange(list);
            return list;
        }

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

        private IDbTransaction BeginTransaction()
        {
            return DbService.Instance.Db.BeginTransaction(IsolationLevel.Serializable);
        }
    }
}
