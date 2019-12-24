using Autofac;
using SiamCross.AppObjects;
using SiamCross.DataBase;
using SiamCross.DataBase.DataBaseModels;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public class DataRepository
    {
        private static readonly Lazy<DataRepository> _instance =
            new Lazy<DataRepository>(() => new DataRepository());
        public static DataRepository Instance { get => _instance.Value; }
        private SQLiteConnection _database;
        private static object _locker = new object();
        private string _databasePart;

        private DataRepository()
        {
            string _databasePart = AppContainer.Container.Resolve<ISQLite>()
                .GetDatabasePath("test.db");
            //using (_database = new SQLiteConnection(_databasePart))
            //{
            _database = new SQLiteConnection(_databasePart);
                _database.CreateTable<Ddim2Measurement>();
              //  _database.Close();
           // }
        }

        public IEnumerable<Ddim2Measurement> GetDdim2Items()
        {
            List<Ddim2Measurement> table;

            Monitor.Enter(_locker);
            //using (_database = new SQLiteConnection(_databasePart))
         //   { 
                table = _database.Table<Ddim2Measurement>().ToList();
//_database.Close();
         //   }
            Monitor.Exit(_locker);
            return table;
        }

        public Ddim2Measurement GetDdimItem(int id)
        {
            Ddim2Measurement measurement;

            Monitor.Enter(_locker);
         //   using (_database = new SQLiteConnection(_databasePart))
          //  {
               
                measurement = _database.Get<Ddim2Measurement>(id);            
          //      _database.Close();              
         //   }
            Monitor.Exit(_locker);

            return measurement;
        }

        public int DeleteDdim2Item(int id)
        {
            int result;

            Monitor.Enter(_locker);
        //    using (_database = new SQLiteConnection(_databasePart))
       //     {             
                result = _database.Delete<Ddim2Measurement>(id);
           //     _database.Close();
           // }
            Monitor.Exit(_locker);
            return result;
        }

        public int SaveDdim2Item(Ddim2Measurement item)
        {
            int result;

            Monitor.Enter(_locker);
         //   using (_database = new SQLiteConnection(_databasePart))
          //  {
                if (item.Id != 0)
                {
                    _database.Update(item);
                    result = item.Id;
                }
                else
                {
                    result = _database.Insert(item);
                }
        //        _database.Close();
         //   }
           Monitor.Exit(_locker);

            return result;
        }
    }
}

