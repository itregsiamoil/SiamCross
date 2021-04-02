using SiamCross.Services.RepositoryTables;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public class FieldDir
    {
        readonly public Dictionary<string, FieldItem> DictByTitle = new Dictionary<string, FieldItem>();
        readonly public Dictionary<uint, FieldItem> DictById = new Dictionary<uint, FieldItem>();
        readonly public ObservableCollection<FieldItem> FieldList = new ObservableCollection<FieldItem>();

        public async Task Init()
        {
            DictByTitle.Clear();
            DictById.Clear();
            FieldList.Clear();

            var values = await DbService.Instance.FieldDictionary.Load();
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
            var val = await DbService.Instance.FieldDictionary.Save(title, id);
            DictByTitle.Add(title, val);
            DictById.Add(id, val);
            FieldList.Add(val);
        }
        public async Task<FieldItem> DeleteAsync(string title)
        {
            if (!DictByTitle.TryGetValue(title, out FieldItem val))
                return null;
            await DbService.Instance.FieldDictionary.Delete(val.Id);
            DictByTitle.Remove(title);
            DictById.Remove(val.Id);
            FieldList.Remove(val);
            return val;
        }
        public async Task<FieldItem> DeleteAsync(uint id)
        {
            if (!DictById.TryGetValue(id, out FieldItem val))
                return null;
            await DbService.Instance.FieldDictionary.Delete(id);
            DictByTitle.Remove(val.Title);
            DictById.Remove(id);
            FieldList.Remove(val);
            return val;
        }
    }
}
