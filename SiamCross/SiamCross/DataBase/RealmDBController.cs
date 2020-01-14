using Realms;
using SiamCross.DataBase.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiamCross.DataBase
{
    public class RealmDBController : IDisposable
    {
        private Realm _realm;
        private Transaction _transaction;

        public RealmDBController()
        {
            _realm = Realm.GetInstance();
            _transaction = _realm.BeginWrite();
        }

        /// ***** /// DDIM2
        public void AddDdim2(Ddim2Measurement ddim2Measurement)
        {
            ddim2Measurement.Id = _realm.All<Ddim2Measurement>().Count();
            _realm.Add(ddim2Measurement);

            _transaction.Commit();
        }
        public void RemoveDdim2(Ddim2Measurement ddim2Measurement)
        {
            _realm.Remove(ddim2Measurement);

            _transaction.Commit();
        }

        public Ddim2Measurement GetDdim2(int id)
        {
            _transaction = _realm.BeginWrite();
            return _realm.Find<Ddim2Measurement>(id);
        }

        public IQueryable<Ddim2Measurement> GetAllDdim2()
        {
            _transaction = _realm.BeginWrite();
            return _realm.All<Ddim2Measurement>();
        }

        public void RemoveDdim2FromId(int id)
        {
            RemoveDdim2(GetDdim2(id));
            _transaction.Commit();
        }

        /// ***** ///

        /// ***** /// DDIN2
        public void AddDdin2(Ddin2Measurement ddin2Measurement)
        {
            ddin2Measurement.Id = _realm.All<Ddin2Measurement>().Count();
            _realm.Add(ddin2Measurement);

            _transaction.Commit();
        }
        public void RemoveDdin2(Ddin2Measurement ddin2Measurement)
        {
            _realm.Remove(ddin2Measurement);
            _transaction.Commit();
        }

        public Ddin2Measurement GetDdin2(int id)
        {
            _transaction = _realm.BeginWrite();
            return _realm.Find<Ddin2Measurement>(id);
        }

        public IQueryable<Ddin2Measurement> GetAllDdin2()
        {
            _transaction = _realm.BeginWrite();
            return _realm.All<Ddin2Measurement>();
        }

        public void RemoveDdin2FromId(int id)
        {
            RemoveDdin2(GetDdin2(id));
            _transaction.Commit();
        }

        /// ***** ///
        public void Dispose()
        {
            _transaction.Dispose();
        }
    }
}
