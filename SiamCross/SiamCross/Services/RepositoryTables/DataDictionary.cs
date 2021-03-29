using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SiamCross.Models;

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
        ObservableCollection<DataDictionaryItem> _Values = new ObservableCollection<DataDictionaryItem>();
        public ObservableCollection<DataDictionaryItem> Values { get=> _Values; }

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
