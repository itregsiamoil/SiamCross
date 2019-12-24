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
        private SQLiteAsyncConnection _database;
        private static object _locker = new object();
        

        private DataRepository()
        {
            string databasePath = AppContainer.Container.Resolve<ISQLite>()
                .GetDatabasePath("sqlite.db");
            _database = new SQLiteAsyncConnection(databasePath);
            _database.CreateTableAsync<Ddim2Measurement>();
        }

        public async Task<IEnumerable<Ddim2Measurement>> GetDdim2Items()
        {
            List<Ddim2Measurement> table;

            Monitor.Enter(_locker);
            try
            {
                table = await _database.Table<Ddim2Measurement>().ToListAsync();
            }
            finally
            {
                Monitor.Exit(_locker);
            }

            return table;
        }

        public async Task<Ddim2Measurement> GetDdimItem(int id)
        {
            Ddim2Measurement measurement;

            Monitor.Enter(_locker);
            try
            {
                measurement = await _database.GetAsync<Ddim2Measurement>(id);
            }
            finally
            {
                Monitor.Exit(_locker);
            }

            return measurement;
        }

        public async Task<int> DeleteDdim2Item(int id)
        {
            int result;

            Monitor.Enter(_locker);
            try
            { 
                result = await _database.DeleteAsync<Ddim2Measurement>(id);
            }
            finally
            {
                Monitor.Exit(_locker);
            }

            return result;
        }

        public async Task<int> SaveDdim2Item(Ddim2Measurement item)
        {
            int result;

            Monitor.Enter(_locker);
            try
            {
                if (item.Id != 0)
                {
                    await _database.UpdateAsync(item);
                    result = item.Id;
                }
                else
                {
                    result = await _database.InsertAsync(item);
                }
            }
            finally
            {
                Monitor.Exit(_locker);
            }

            return result;
        }
    }
}

