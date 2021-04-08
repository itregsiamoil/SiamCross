using Dapper;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public class DataDictionaryItem
    {
        public string Title { get; set; }
        public int Kind { get; set; }
        public int Tag { get; set; }
    }
    public class DataDictionary
    {
        readonly ObservableCollection<DataDictionaryItem> _Values = new ObservableCollection<DataDictionaryItem>();
        public ObservableCollection<DataDictionaryItem> Values => _Values;

        private readonly IDbConnection _db;
        public DataDictionary(IDbConnection db)
        {
            _db = db;
        }

        public async Task Load()
        {
            _Values.Clear();
            var values = await _db.QueryAsync<DataDictionaryItem>(
                "SELECT * FROM DataDictionary");
            foreach (var v in values)
                _Values.Add(v);
        }
    }
}
