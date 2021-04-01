using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public class FieldDir
    {
        readonly private Dictionary<string, uint> _DictByTitle = new Dictionary<string, uint>();
        readonly private Dictionary<uint, string> _DictById = new Dictionary<uint, string>();
        readonly private ObservableCollection<string> _FieldList = new ObservableCollection<string>();

        public Dictionary<string, uint> DictByTitle => _DictByTitle;
        public Dictionary<uint, string> DictById => _DictById;
        public ObservableCollection<string> TitleList => _FieldList;

        public async Task Init()
        {
            DictByTitle.Clear();
            DictById.Clear();
            TitleList.Clear();

            var values = await DbService.Instance.FieldDictionary.Load();
            foreach (var v in values)
            {
                DictByTitle.Add(v.Title, v.Id);
                DictById.Add(v.Id, v.Title);
                TitleList.Add(v.Title);
            }
        }
        public async Task AddAsync(string title, uint id)
        {
            await DbService.Instance.FieldDictionary.Save(title, id);
            if (DictByTitle.ContainsKey(title) || DictById.ContainsKey(id))
            {
                DictById.Remove(id);
                DictByTitle.Remove(title);
                TitleList.Remove(title);
            }
            DictByTitle.Add(title, id);
            DictById.Add(id, title);
            TitleList.Add(title);
        }
        public async Task DeleteAsync(string title)
        {
            if (!DictByTitle.ContainsKey(title))
                return;
            if (DictByTitle.TryGetValue(title, out uint id))
            {
                await DbService.Instance.FieldDictionary.Delete(id);
                DictById.Remove(id);
                DictByTitle.Remove(title);
                TitleList.Remove(title);
            }
        }
    }
}
