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

        public async Task InitAsync()
        {
            ById.Clear();
            ByTitle.Clear();
            IEnumerable<AttributeItem> values;
            using (var tr = BeginTransaction())
                values = await LoadAsync(tr);
            foreach (var v in values)
            {
                ById.Add(v.Id, v);
                ByTitle.Add(v.Title, v);
            }
        }

        private async Task<IEnumerable<AttributeItem>> LoadAsync(IDbTransaction tr)
        {
            var values = await tr.Connection.QueryAsync<AttributeItem>(
                "SELECT * FROM Attributes");
            return values;
        }
        public async Task<AttributeItem> SaveAsync(string title)
        {
            var item = new FieldItem()
            {
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
        IDbTransaction BeginTransaction()
        {
            return DbService.Instance.Db.BeginTransaction(IsolationLevel.Serializable);
        }
    }
}
