using SiamCross.Services.RepositoryTables;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public class FieldDir
    {
        public readonly Dictionary<string, FieldItem> DictByTitle = new Dictionary<string, FieldItem>();
        public readonly Dictionary<uint, FieldItem> DictById = new Dictionary<uint, FieldItem>();
        public readonly ObservableCollection<FieldItem> FieldList = new ObservableCollection<FieldItem>();

        public async Task InitAsync()
        {
            DictByTitle.Clear();
            DictById.Clear();
            FieldList.Clear();

            var values = await DbService.Instance.LoadFieldAsync();
            foreach (var v in values)
            {
                DictByTitle.Add(v.Title, v);
                DictById.Add(v.Id, v);
                FieldList.Add(v);
            }
        }
        public async Task AddAsync(string title, uint id)
        {
            await DeleteAsync(id);
            await DeleteAsync(title);
            var val = await DbService.Instance.SaveFieldAsync(title, id);
            DictByTitle.Add(title, val);
            DictById.Add(id, val);
            FieldList.Add(val);
        }
        public async Task<FieldItem> DeleteAsync(string title)
        {
            if (!DictByTitle.TryGetValue(title, out FieldItem val))
                return null;
            await DbService.Instance.DelFieldAsync(val.Id);
            DictByTitle.Remove(title);
            DictById.Remove(val.Id);
            FieldList.Remove(val);
            return val;
        }
        public async Task<FieldItem> DeleteAsync(uint id)
        {
            if (!DictById.TryGetValue(id, out FieldItem val))
                return null;
            await DbService.Instance.DelFieldAsync(id);
            DictByTitle.Remove(val.Title);
            DictById.Remove(id);
            FieldList.Remove(val);
            return val;
        }
    }
}
