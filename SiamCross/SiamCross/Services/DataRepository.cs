using Autofac;
using SiamCross.AppObjects;
using SiamCross.DataBase;
using SiamCross.DataBase.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SQLite;

namespace SiamCross.Services
{
    public class DataRepository
    {
        private static readonly Lazy<DataRepository> _instance =
            new Lazy<DataRepository>(() => new DataRepository());
        public static DataRepository Instance { get => _instance.Value; }
        private SQLiteConnection _database;
        private static object _locker = new object();

        private DataRepository()
        {
            string _databasePart = AppContainer.Container.Resolve<ISQLite>()
                .GetDatabasePath("sqlite.db");
            try
            {
                _database = new SQLiteConnection(_databasePart);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            _database.CreateTable<Ddim2Measurement>();
            _database.CreateTable<Ddin2Measurement>();
        }

        public IEnumerable<Ddim2Measurement> GetDdim2Items()
        {
            List<Ddim2Measurement> table;

            Monitor.Enter(_locker);

            table = _database.Table<Ddim2Measurement>().ToList();

            Monitor.Exit(_locker);
            return table;
        }

        public Ddim2Measurement GetDdim2Item(int id)
        {
            Ddim2Measurement measurement;

            Monitor.Enter(_locker);


            measurement = _database.Get<Ddim2Measurement>(id);

            Monitor.Exit(_locker);

            return measurement;
        }

        public int DeleteDdim2Item(int id)
        {
            int result;

            Monitor.Enter(_locker);

            result = _database.Delete<Ddim2Measurement>(id);

            Monitor.Exit(_locker);
            return result;
        }

        public int SaveDdim2Item(Ddim2Measurement item)
        {
            int result;

            Monitor.Enter(_locker);

            if (item.Id != 0)
            {
                _database.Update(item);
                result = item.Id;
            }
            else
            {
                result = _database.Insert(item);
            }

            Monitor.Exit(_locker);

            return result;
        }

        public IEnumerable<Ddin2Measurement> GetDdin2Items()
        {
            List<Ddin2Measurement> table;

            Monitor.Enter(_locker);

            table = _database.Table<Ddin2Measurement>().ToList();

            Monitor.Exit(_locker);
            return table;
        }

        public Ddin2Measurement GetDdin2Item(int id)
        {
            Ddin2Measurement measurement;

            Monitor.Enter(_locker);


            measurement = _database.Get<Ddin2Measurement>(id);

            Monitor.Exit(_locker);

            return measurement;
        }

        public int DeleteDdin2Item(int id)
        {
            int result;

            Monitor.Enter(_locker);

            result = _database.Delete<Ddin2Measurement>(id);

            Monitor.Exit(_locker);
            return result;
        }

        public int SaveDdin2Item(Ddin2Measurement item)
        {
            int result;

            Monitor.Enter(_locker);

            if (item.Id != 0)
            {
                _database.Update(item);
                result = item.Id;
            }
            else
            {
                result = _database.Insert(item);
            }

            Monitor.Exit(_locker);

            return result;
        }
    }
}

