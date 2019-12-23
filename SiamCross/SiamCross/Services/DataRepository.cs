using Autofac;
using SiamCross.AppObjects;
using SiamCross.DataBase;
using SiamCross.DataBase.DataBaseModels;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiamCross.Services
{
    public class DataRepository
    {
        private static readonly Lazy<DataRepository> _instance =
            new Lazy<DataRepository>(() => new DataRepository());

        private DataRepository()
        {
            string databasePath = AppContainer.Container.Resolve<ISQLite>().GetDatabasePath("sqlite.db");
            _database = new SQLiteConnection(databasePath);
            _database.CreateTable<Ddim2Measurement>();
        }

        private SQLiteConnection _database;
        public static DataRepository Instance { get => _instance.Value; }

        public IEnumerable<Ddim2Measurement> GetDdim2Items<T>()
        {
            return (from i in _database.Table<Ddim2Measurement>() select i).ToList();
        }
        public Ddim2Measurement GetDdimItem<T>(int id)
        {
            return _database.Get<Ddim2Measurement>(id);
        }
        public int DeleteDdim2Item(int id)
        {
            return _database.Delete<Ddim2Measurement>(id);
        }
        public int SaveDdim2Item(Ddim2Measurement item)
        {
            if (item.Id != 0)
            {
                _database.Update(item);
                return item.Id;
            }
            else
            {
                return _database.Insert(item);
            }
        }
    }
}
