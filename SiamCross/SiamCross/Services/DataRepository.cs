using Autofac;
using Dapper;
using Mono.Data.Sqlite;
using SiamCross.AppObjects;
using SiamCross.DataBase;
using SiamCross.DataBase.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
        private SqliteConnection _database;
        private static object _locker = new object();

        private DataRepository()
        {
            string _databasePart = AppContainer.Container.Resolve<ISQLite>()
                .GetDatabasePath("Db.sqlite");

            if (!File.Exists(_databasePart))
            {
                SqliteConnection.CreateFile(_databasePart);
            }
            _database = new SqliteConnection(string.Format(
                "Data Source={0};Version=3;", _databasePart));

            _database.Open();
            CreateDdim2Table();
            CreateDdin2Table();
        }

        private void NonQueryCheck()
        {
            // Ensure we have a connection
            if (_database == null)
            {
                throw new NullReferenceException("Please provide a connection");
            }

            // Ensure that the connection state is Open
            if (_database.State != ConnectionState.Open)
            {
                _database.Open();
            }
        }

        private void CreateDdim2Table()
        {
            NonQueryCheck();

            _database.Execute(@"
            CREATE TABLE IF NOT EXISTS [Ddim2Measurement] (
                [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                [MaxWeight] REAL NOT NULL,
                [MinWeight] REAL NOT NULL,
                [Travel] REAL NOT NULL,
                [Period] REAL NOT NULL,
                [Step] REAL NOT NULL,
                [WeightDiscr] REAL NOT NULL,
                [TimeDiscr] REAL NOT NULL,
                [DynGraph] BLOB,
                [AccelerationGraph] BLOB,
                [Field] NVARCHAR(128) NOT NULL,
                [Well] NVARCHAR(128) NOT NULL,
                [Bush] NVARCHAR(128) NOT NULL,
                [Shop] NVARCHAR(128) NOT NULL,
                [BufferPressure] NVARCHAR(128) NOT NULL,
                [Comment] NVARCHAR(128) NOT NULL,
                [Name] NVARCHAR(128) NOT NULL,
                [DateTime] TEXT NOT NULL,
                [ErrorCode] NVARCHAR(128),
                [ApertNumber] REAL NOT NULL,
                [ModelPump] REAL NOT NULL,
                [MaxBarbellWeight] REAL NOT NULL,
                [MinBarbellWeight] REAL NOT NULL,
                [TravelLength] REAL NOT NULL,
                [SwingCount] REAL NOT NULL
            )");
        }

        private void CreateDdin2Table()
        {
            NonQueryCheck();

            _database.Execute(@"
            CREATE TABLE IF NOT EXISTS [Ddin2Measurement] (
                [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                [MaxWeight] REAL NOT NULL,
                [MinWeight] REAL NOT NULL,
                [Travel] REAL NOT NULL,
                [Period] REAL NOT NULL,
                [Step] REAL NOT NULL,
                [WeightDiscr] REAL NOT NULL,
                [TimeDiscr] REAL NOT NULL,
                [DynGraph] BLOB,
                [AccelerationGraph] BLOB,
                [Field] NVARCHAR(128) NOT NULL,
                [Well] NVARCHAR(128) NOT NULL,
                [Bush] NVARCHAR(128) NOT NULL,
                [Shop] NVARCHAR(128) NOT NULL,
                [BufferPressure] NVARCHAR(128) NOT NULL,
                [Comment] NVARCHAR(128) NOT NULL,
                [Name] NVARCHAR(128) NOT NULL,
                [DateTime] TEXT NOT NULL,
                [ErrorCode] NVARCHAR(128),
                [ApertNumber] REAL NOT NULL,
                [ModelPump] REAL NOT NULL,
                [Rod] REAL NOT NULL,
                [MaxBarbellWeight] REAL NOT NULL,
                [MinBarbellWeight] REAL NOT NULL,
                [TravelLength] REAL NOT NULL,
                [SwingCount] REAL NOT NULL
            )");
        }

        public int SaveDdim2Measurement(Ddim2Measurement ddim2Measurement)
        {
            NonQueryCheck();

            if(ddim2Measurement.Id == 0)
            {
                ddim2Measurement.Id = (int)_database.Query(string.Format(
                    "SELECT COUNT(1) as 'Count' FROM Ddim2Measurement")).First().Count + 1;
            }

            var rows = _database.Query(string.Format(
            "SELECT COUNT(1) as 'Count' FROM Ddim2Measurement WHERE Id = '{0}'",
            ddim2Measurement.Id));

            if (rows.First().Count > 0)
            {
                UpdateDdim2Measurement(ddim2Measurement);
                return ddim2Measurement.Id;
            }

            string sql = "INSERT INTO Ddim2Measurement" +
                    "(MaxWeight, MinWeight, Travel, Period, Step, WeightDiscr, TimeDiscr, DynGraph," +
                    " AccelerationGraph, Field, Well, Bush, Shop, BufferPressure, Comment, " +
                    "Name, DateTime, ErrorCode, ApertNumber, ModelPump, MaxBarbellWeight," +
                    " MinBarbellWeight, TravelLength, SwingCount) Values (@MaxWeight, @MinWeight," +
                    " @Travel, @Period, @Step, @WeightDiscr, @TimeDiscr, @DynGraph," +
                    " @AccelerationGraph, @Field, @Well, @Bush, @Shop, @BufferPressure, @Comment, " +
                    "@Name, @DateTime, @ErrorCode, @ApertNumber, @ModelPump, @MaxBarbellWeight," +
                    " @MinBarbellWeight, @TravelLength, @SwingCount);";
            
                var affectedRows = _database.Execute(sql, new
                {
                    MaxWeight = ddim2Measurement.MaxWeight,
                    MinWeight = ddim2Measurement.MinWeight,
                    Travel = ddim2Measurement.Travel,
                    Period = ddim2Measurement.Period,
                    Step = ddim2Measurement.Step,
                    WeightDiscr = ddim2Measurement.WeightDiscr,
                    TimeDiscr = ddim2Measurement.TimeDiscr,
                    DynGraph = ddim2Measurement.DynGraph,
                    AccelerationGraph = ddim2Measurement.AccelerationGraph,
                    Field = ddim2Measurement.Field,
                    Well = ddim2Measurement.Well,
                    Bush = ddim2Measurement.Bush,
                    Shop = ddim2Measurement.Shop,
                    BufferPressure = ddim2Measurement.BufferPressure,
                    Comment = ddim2Measurement.Comment,
                    Name = ddim2Measurement.Name,
                    DateTime = ddim2Measurement.DateTime,
                    ErrorCode = ddim2Measurement.ErrorCode,
                    ApertNumber = ddim2Measurement.ApertNumber,
                    ModelPump = ddim2Measurement.ModelPump,
                    MaxBarbellWeight = ddim2Measurement.MaxBarbellWeight,
                    MinBarbellWeight = ddim2Measurement.MinBarbellWeight,
                    TravelLength = ddim2Measurement.TravelLength,
                    SwingCount = ddim2Measurement.SwingCount
            });

            return ddim2Measurement.Id;
        }

        private void UpdateDdim2Measurement(Ddim2Measurement ddim2Measurement)
        {
            string sql = "UPDATE Ddim2Measurement SET " +
                "MaxWeight = @MaxWeight, MinWeight = @MinWeight, Travel = @Travel," +
                " Period = @Period, Step = @Step, WeightDiscr = @WeightDiscr," +
                " TimeDiscr = @TimeDiscr, DynGraph = @DynGraph," +
                " AccelerationGraph = @AccelerationGraph, Field = @Field," +
                " Well = @Well, Bush = @Bush, Shop = @Shop, BufferPressure = @BufferPressure," +
                " Comment = @Comment, Name = @Name, DateTime = @DateTime," +
                " ErrorCode = @ErrorCode, ApertNumber = @ApertNumber," +
                " ModelPump = @ModelPump, MaxBarbellWeight = @MaxBarbellWeight," +
                " MinBarbellWeight = @MinBarbellWeight, TravelLength = @TravelLength, SwingCount = @SwingCount" +
                " WHERE Id = @Id;";

            var affectedRows = _database.Execute(sql, new
            {
                MaxWeight = ddim2Measurement.MaxWeight,
                MinWeight = ddim2Measurement.MinWeight,
                Travel = ddim2Measurement.Travel,
                Period = ddim2Measurement.Period,
                Step = ddim2Measurement.Step,
                WeightDiscr = ddim2Measurement.WeightDiscr,
                TimeDiscr = ddim2Measurement.TimeDiscr,
                DynGraph = ddim2Measurement.DynGraph,
                AccelerationGraph = ddim2Measurement.AccelerationGraph,
                Field = ddim2Measurement.Field,
                Well = ddim2Measurement.Well,
                Bush = ddim2Measurement.Bush,
                Shop = ddim2Measurement.Shop,
                BufferPressure = ddim2Measurement.BufferPressure,
                Comment = ddim2Measurement.Comment,
                Name = ddim2Measurement.Name,
                DateTime = ddim2Measurement.DateTime,
                ErrorCode = ddim2Measurement.ErrorCode,
                ApertNumber = ddim2Measurement.ApertNumber,
                ModelPump = ddim2Measurement.ModelPump,
                MaxBarbellWeight = ddim2Measurement.MaxBarbellWeight,
                MinBarbellWeight = ddim2Measurement.MinBarbellWeight,
                TravelLength = ddim2Measurement.TravelLength,
                SwingCount = ddim2Measurement.SwingCount,
                Id = ddim2Measurement.Id
            });
        }

        public void RemoveDdim2Measurement(int removebleId)
        {
            NonQueryCheck();

            _database.Execute("DELETE FROM Ddim2Measurement WHERE Id =" + removebleId);
        }

        public IEnumerable<Ddim2Measurement> GetDdim2Measurements()
        {
            NonQueryCheck();
            return _database.Query<Ddim2Measurement>(
                "SELECT * FROM Ddim2Measurement");
        }

        public Ddim2Measurement GetDdim2MeasurementById(int id)
        {
            NonQueryCheck();
            return _database.Query<Ddim2Measurement>(
                "SELECT * FROM Ddim2Measurement WHERE Id =" + id).First();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public int SaveDdin2Measurement(Ddin2Measurement ddin2Measurement)
        {
            NonQueryCheck();

            if (ddin2Measurement.Id == 0)
            {
                ddin2Measurement.Id = (int)_database.Query(string.Format(
                "SELECT COUNT(1) as 'Count' FROM Ddin2Measurement")).First().Count + 1;
            }

            var rows = _database.Query(string.Format(
                "SELECT COUNT(1) as 'Count' FROM Ddin2Measurement WHERE Id = '{0}'",
                ddin2Measurement.Id));

            if (rows.First().Count > 0)
            {
                UpdateDdin2Measurement(ddin2Measurement);
                return ddin2Measurement.Id;
            }

            string sql = "INSERT INTO Ddin2Measurement" +
                    "(MaxWeight, MinWeight, Travel, Period, Step, WeightDiscr, TimeDiscr, DynGraph," +
                    " AccelerationGraph, Field, Well, Bush, Shop, BufferPressure, Comment, " +
                    "Name, DateTime, ErrorCode, ApertNumber, ModelPump, Rod, MaxBarbellWeight," +
                    " MinBarbellWeight, TravelLength, SwingCount) Values (@MaxWeight, @MinWeight," +
                    " @Travel, @Period, @Step, @WeightDiscr, @TimeDiscr, @DynGraph," +
                    " @AccelerationGraph, @Field, @Well, @Bush, @Shop, @BufferPressure, @Comment, " +
                    "@Name, @DateTime, @ErrorCode, @ApertNumber, @ModelPump, @Rod, @MaxBarbellWeight," +
                    " @MinBarbellWeight, @TravelLength, @SwingCount);";

            var affectedRows = _database.Execute(sql, new
            {
                MaxWeight = ddin2Measurement.MaxWeight,
                MinWeight = ddin2Measurement.MinWeight,
                Travel = ddin2Measurement.Travel,
                Period = ddin2Measurement.Period,
                Step = ddin2Measurement.Step,
                WeightDiscr = ddin2Measurement.WeightDiscr,
                TimeDiscr = ddin2Measurement.TimeDiscr,
                DynGraph = ddin2Measurement.DynGraph,
                AccelerationGraph = ddin2Measurement.AccelerationGraph,
                Field = ddin2Measurement.Field,
                Well = ddin2Measurement.Well,
                Bush = ddin2Measurement.Bush,
                Shop = ddin2Measurement.Shop,
                BufferPressure = ddin2Measurement.BufferPressure,
                Comment = ddin2Measurement.Comment,
                Name = ddin2Measurement.Name,
                DateTime = ddin2Measurement.DateTime,
                ErrorCode = ddin2Measurement.ErrorCode,
                ApertNumber = ddin2Measurement.ApertNumber,
                ModelPump = ddin2Measurement.ModelPump,
                Rod = ddin2Measurement.Rod,
                MaxBarbellWeight = ddin2Measurement.MaxBarbellWeight,
                MinBarbellWeight = ddin2Measurement.MinBarbellWeight,
                TravelLength = ddin2Measurement.TravelLength,
                SwingCount = ddin2Measurement.SwingCount
            });

            return ddin2Measurement.Id;
        }

        private void UpdateDdin2Measurement(Ddin2Measurement ddin2Measurement)
        {
            string sql = "UPDATE Ddin2Measurement SET " +
                 "MaxWeight = @MaxWeight, MinWeight = @MinWeight, Travel = @Travel," +
                 " Period = @Period, Step = @Step, WeightDiscr = @WeightDiscr," +
                 " TimeDiscr = @TimeDiscr, DynGraph = @DynGraph," +
                 " AccelerationGraph = @AccelerationGraph, Field = @Field," +
                 " Well = @Well, Bush = @Bush, Shop = @Shop, BufferPressure = @BufferPressure," +
                 " Comment = @Comment, Name = @Name, DateTime = @DateTime," +
                 " ErrorCode = @ErrorCode, ApertNumber = @ApertNumber," +
                 " ModelPump = @ModelPump, Rod = @Rod, MaxBarbellWeight = @MaxBarbellWeight," +
                 " MinBarbellWeight = @MinBarbellWeight, TravelLength = @TravelLength, SwingCount = @SwingCount" +
                 " WHERE Id = @Id;";

            var affectedRows = _database.Execute(sql, new
            {
                MaxWeight = ddin2Measurement.MaxWeight,
                MinWeight = ddin2Measurement.MinWeight,
                Travel = ddin2Measurement.Travel,
                Period = ddin2Measurement.Period,
                Step = ddin2Measurement.Step,
                WeightDiscr = ddin2Measurement.WeightDiscr,
                TimeDiscr = ddin2Measurement.TimeDiscr,
                DynGraph = ddin2Measurement.DynGraph,
                AccelerationGraph = ddin2Measurement.AccelerationGraph,
                Field = ddin2Measurement.Field,
                Well = ddin2Measurement.Well,
                Bush = ddin2Measurement.Bush,
                Shop = ddin2Measurement.Shop,
                BufferPressure = ddin2Measurement.BufferPressure,
                Comment = ddin2Measurement.Comment,
                Name = ddin2Measurement.Name,
                DateTime = ddin2Measurement.DateTime,
                ErrorCode = ddin2Measurement.ErrorCode,
                ApertNumber = ddin2Measurement.ApertNumber,
                ModelPump = ddin2Measurement.ModelPump,
                Rod = ddin2Measurement.Rod,
                MaxBarbellWeight = ddin2Measurement.MaxBarbellWeight,
                MinBarbellWeight = ddin2Measurement.MinBarbellWeight,
                TravelLength = ddin2Measurement.TravelLength,
                SwingCount = ddin2Measurement.SwingCount,
                Id = ddin2Measurement.Id
            });
        }

        public void RemoveDdin2Measurement(int removebleId)
        {
            NonQueryCheck();

            _database.Execute("DELETE FROM Ddin2Measurement WHERE Id =" + removebleId);
        }

        public IEnumerable<Ddin2Measurement> GetDdin2Measurements()
        {
            NonQueryCheck();
            return _database.Query<Ddin2Measurement>(
                "SELECT * FROM Ddin2Measurement");
        }

        public Ddin2Measurement GetDdin2MeasurementById(int id)
        {
            NonQueryCheck();
            return _database.Query<Ddin2Measurement>(
                "SELECT * FROM Ddin2Measurement WHERE Id =" + id).First();
        }
    }
}


